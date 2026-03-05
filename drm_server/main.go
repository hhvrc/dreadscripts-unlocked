// drm_server — local HTTPS server that spoofs the DreadScripts Firebase DRM endpoint.
//
// The DLL POSTs JSON to:
//   https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand
//
// This server accepts all license requests and always returns success=true.
//
// Usage:
//   drm_server serve                   # listen on :443 (requires admin)
//   drm_server serve --addr :8443      # non-privileged port
//   drm_server install-cert            # install cert into Windows Trusted Root (requires admin)
//   drm_server export-cert cert.pem    # write TLS cert PEM to a file
//   drm_server patch-hosts             # append DRM hostname to hosts file (requires admin)
//
// The cert is persisted to drm-server-cert.pem / drm-server-key.pem on first run
// and reused on subsequent runs, so the installed trust stays valid.

package main

import (
	"crypto/tls"
	"fmt"
	"log"
	"net"
	"net/http"
	"os"

	"github.com/spf13/cobra"
	"golang.org/x/sys/windows/svc"
)

// drmHost is the Firebase Cloud Function hostname the DLL connects to.
const drmHost = "us-central1-dreadscripts-c6b62.cloudfunctions.net"

func main() {
	// When the SCM starts the binary it runs as a service, not interactively.
	// Detect this early and hand off to the service dispatcher.
	if isService, _ := svc.IsWindowsService(); isService {
		if err := runAsService(); err != nil {
			os.Exit(1)
		}
		return
	}

	root := &cobra.Command{
		Use:   "drm_server",
		Short: "Local HTTPS server that spoofs the DreadScripts DRM endpoint",
	}

	// ── serve ──────────────────────────────────────────────────────────────────
	var addr string
	serveCmd := &cobra.Command{
		Use:   "serve",
		Short: "Start the DRM HTTPS server",
		Args:  cobra.NoArgs,
		RunE: func(cmd *cobra.Command, args []string) error {
			return runServe(addr)
		},
	}
	serveCmd.Flags().StringVar(&addr, "addr", ":443", "Listen address (use :8443 if not running as admin)")
	root.AddCommand(serveCmd)

	// ── install-cert ───────────────────────────────────────────────────────────
	installCertCmd := &cobra.Command{
		Use:   "install-cert",
		Short: "Install the self-signed cert into the Windows Trusted Root store (requires admin)",
		Args:  cobra.NoArgs,
		RunE: func(cmd *cobra.Command, args []string) error {
			certPEM, _, _, err := loadOrGenerateCert()
			if err != nil {
				return fmt.Errorf("cert: %w", err)
			}
			return installCert(certPEM)
		},
	}
	root.AddCommand(installCertCmd)

	// ── export-cert ────────────────────────────────────────────────────────────
	exportCertCmd := &cobra.Command{
		Use:   "export-cert <file>",
		Short: "Write the self-signed cert PEM to a file",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			certPEM, _, _, err := loadOrGenerateCert()
			if err != nil {
				return fmt.Errorf("cert: %w", err)
			}
			if err := os.WriteFile(args[0], certPEM, 0644); err != nil {
				return fmt.Errorf("write cert: %w", err)
			}
			log.Printf("cert written to %s", args[0])
			return nil
		},
	}
	root.AddCommand(exportCertCmd)

	// ── patch-hosts ────────────────────────────────────────────────────────────
	patchHostsCmd := &cobra.Command{
		Use:   "patch-hosts",
		Short: "Append DRM hostname → 127.0.0.1 to the system hosts file (requires admin)",
		Args:  cobra.NoArgs,
		RunE: func(cmd *cobra.Command, args []string) error {
			return patchHosts()
		},
	}
	root.AddCommand(patchHostsCmd)

	// ── install-service ────────────────────────────────────────────────────────
	root.AddCommand(&cobra.Command{
		Use:   "install-service",
		Short: "Register as a Windows auto-start service (requires Administrator)",
		Args:  cobra.NoArgs,
		RunE:  func(cmd *cobra.Command, args []string) error { return installService() },
	})

	// ── uninstall-service ──────────────────────────────────────────────────────
	root.AddCommand(&cobra.Command{
		Use:   "uninstall-service",
		Short: "Stop and remove the Windows service (requires Administrator)",
		Args:  cobra.NoArgs,
		RunE:  func(cmd *cobra.Command, args []string) error { return uninstallService() },
	})

	// ── start-service ──────────────────────────────────────────────────────────
	root.AddCommand(&cobra.Command{
		Use:   "start-service",
		Short: "Ask the SCM to start the service (requires Administrator)",
		Args:  cobra.NoArgs,
		RunE:  func(cmd *cobra.Command, args []string) error { return startService() },
	})

	// ── stop-service ───────────────────────────────────────────────────────────
	root.AddCommand(&cobra.Command{
		Use:   "stop-service",
		Short: "Ask the SCM to stop the service (requires Administrator)",
		Args:  cobra.NoArgs,
		RunE:  func(cmd *cobra.Command, args []string) error { return stopService() },
	})

	if err := root.Execute(); err != nil {
		os.Exit(1)
	}
}

func runServe(addr string) error {
	certPEM, _, tlsCert, err := loadOrGenerateCert()
	if err != nil {
		return fmt.Errorf("cert: %w", err)
	}
	_ = certPEM

	log.Printf("DreadScripts DRM server")
	log.Printf("  Listening: https://localhost%s/receiveCommand", addr)
	log.Printf("")
	log.Printf("Setup checklist:")
	log.Printf("  1. Hosts file:  drm_server patch-hosts   (as Administrator)")
	log.Printf("     OR manually: echo 127.0.0.1  %s >> %s", drmHost, hostsPath)
	log.Printf("")
	log.Printf("  2. Trust the self-signed cert — choose ONE:")
	log.Printf("     a) drm_server install-cert             (as Administrator, permanent)")
	log.Printf("     b) drm_server export-cert cert.pem  then:")
	log.Printf("          certutil -addstore -f Root cert.pem")
	log.Printf("     c) Harmony-patch UnityWebRequest in Unity to skip cert check")
	log.Printf("")
	log.Printf("Products served:")
	for _, p := range products {
		log.Printf("  %-20s (%s)", p.Name, p.ID)
	}
	log.Printf("")

	tlsCfg := &tls.Config{
		Certificates: []tls.Certificate{tlsCert},
		MinVersion:   tls.VersionTLS12,
	}

	ln, err := net.Listen("tcp", addr)
	if err != nil {
		return fmt.Errorf("listen on %s: %w\n(use --addr :8443 if port 443 requires elevation)", addr, err)
	}
	log.Printf("Listening on https://localhost%s — waiting for requests...", addr)

	mux := http.NewServeMux()
	mux.HandleFunc("/receiveCommand", handleReceiveCommand)
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		log.Printf("  [unknown path] %s %s", r.Method, r.URL.Path)
		http.NotFound(w, r)
	})

	srv := &http.Server{
		Handler:   mux,
		TLSConfig: tlsCfg,
	}

	return srv.Serve(tls.NewListener(ln, tlsCfg))
}
