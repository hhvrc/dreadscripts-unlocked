package main

import (
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/tls"
	"crypto/x509"
	"crypto/x509/pkix"
	"encoding/pem"
	"fmt"
	"log"
	"math/big"
	"net"
	"os"
	"os/exec"
	"time"
)

// generateCert creates an ECDSA P-256 self-signed certificate valid for 10 years,
// covering the DRM hostname and 127.0.0.1.  Returns (certPEM, keyPEM, tlsCert).
func generateCert() ([]byte, []byte, tls.Certificate, error) {
	priv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		return nil, nil, tls.Certificate{}, fmt.Errorf("generate key: %w", err)
	}

	serial, err := rand.Int(rand.Reader, new(big.Int).Lsh(big.NewInt(1), 128))
	if err != nil {
		return nil, nil, tls.Certificate{}, err
	}

	tmpl := &x509.Certificate{
		SerialNumber: serial,
		Subject: pkix.Name{
			Organization: []string{"DreadScripts DRM Server"},
			CommonName:   drmHost,
		},
		DNSNames:              []string{drmHost, "localhost"},
		IPAddresses:           []net.IP{net.ParseIP("127.0.0.1")},
		NotBefore:             time.Now().Add(-time.Minute),
		NotAfter:              time.Now().Add(10 * 365 * 24 * time.Hour),
		KeyUsage:              x509.KeyUsageDigitalSignature | x509.KeyUsageKeyEncipherment,
		ExtKeyUsage:           []x509.ExtKeyUsage{x509.ExtKeyUsageServerAuth},
		IsCA:                  true, // required so Windows trusts it when imported to root store
		BasicConstraintsValid: true,
	}

	certDER, err := x509.CreateCertificate(rand.Reader, tmpl, tmpl, &priv.PublicKey, priv)
	if err != nil {
		return nil, nil, tls.Certificate{}, fmt.Errorf("create cert: %w", err)
	}

	certPEM := pem.EncodeToMemory(&pem.Block{Type: "CERTIFICATE", Bytes: certDER})

	privDER, err := x509.MarshalECPrivateKey(priv)
	if err != nil {
		return nil, nil, tls.Certificate{}, err
	}
	keyPEM := pem.EncodeToMemory(&pem.Block{Type: "EC PRIVATE KEY", Bytes: privDER})

	tlsCert, err := tls.X509KeyPair(certPEM, keyPEM)
	if err != nil {
		return nil, nil, tls.Certificate{}, err
	}

	return certPEM, keyPEM, tlsCert, nil
}

const (
	certFile = "drm-server-cert.pem"
	keyFile  = "drm-server-key.pem"
)

// loadOrGenerateCert loads the cert/key from disk if both files exist,
// otherwise generates new ones and saves them.  This ensures the installed
// cert stays in sync with the cert the server actually presents.
func loadOrGenerateCert() ([]byte, []byte, tls.Certificate, error) {
	certPEM, errC := os.ReadFile(certFile)
	keyPEM, errK := os.ReadFile(keyFile)
	if errC == nil && errK == nil {
		tlsCert, err := tls.X509KeyPair(certPEM, keyPEM)
		if err == nil {
			log.Printf("cert: loaded existing cert from %s / %s", certFile, keyFile)
			return certPEM, keyPEM, tlsCert, nil
		}
		log.Printf("cert: existing files invalid (%v), regenerating", err)
	}

	certPEM, keyPEM, tlsCert, err := generateCert()
	if err != nil {
		return nil, nil, tls.Certificate{}, err
	}
	if err := os.WriteFile(certFile, certPEM, 0644); err != nil {
		log.Printf("cert: warning — could not save %s: %v", certFile, err)
	}
	if err := os.WriteFile(keyFile, keyPEM, 0600); err != nil {
		log.Printf("cert: warning — could not save %s: %v", keyFile, err)
	}
	log.Printf("cert: generated new cert, saved to %s / %s", certFile, keyFile)
	return certPEM, keyPEM, tlsCert, nil
}

// installCert imports certPEM into the Windows "Root" (Trusted Root CA) store
// via certutil.exe.  Requires elevation.
func installCert(certPEM []byte) error {
	tmp, err := os.CreateTemp("", "drm-server-*.crt")
	if err != nil {
		return err
	}
	defer os.Remove(tmp.Name())

	if _, err := tmp.Write(certPEM); err != nil {
		return err
	}
	tmp.Close()

	cmd := exec.Command("certutil", "-addstore", "-f", "Root", tmp.Name())
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	if err := cmd.Run(); err != nil {
		return fmt.Errorf("certutil failed: %w (run as Administrator?)", err)
	}
	log.Printf("cert: installed into Windows Trusted Root store via certutil")
	return nil
}
