package main

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"strings"
	"time"
)

// ── Product registry ──────────────────────────────────────────────────────────

// product holds all per-product configuration extracted from the DLL.
type product struct {
	ID         string // base64 product ID sent in every request
	Name       string // human-readable display name
	HMACPrefix string // secret prefix for QueryServer / ViewProperty HMAC-SHA256 token
	// Key  = HMACPrefix + licenseKey
	// Data = date (DD/MM/YYYY UTC) + HWID
}

// products is the canonical list of known DreadScripts products.
// To support a new product: add one entry here (ID, Name, HMACPrefix).
var products = []product{
	{
		ID:         "No1lKII9IzcBAbihub6nCg==",
		Name:       "ADOverhaul",
		HMACPrefix: "of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?",
	},
	{
		ID:         "yOk0XCnENLMO6DIF8cYpSg==",
		Name:       "ControllerEditor",
		HMACPrefix: "z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[",
	},
}

// productByID returns the product matching the given base64 ID, or a generic
// fallback so callers never have to nil-check.
func productByID(id string) product {
	for _, p := range products {
		if p.ID == id {
			return p
		}
	}
	name := id
	if name == "" {
		name = "unknown"
	}
	return product{ID: id, Name: fmt.Sprintf("unknown(%s)", name)}
}

// token computes the HMAC-SHA256 token the DLL's QueryServer()/ViewProperty()
// validates against the "token" response field.
// Key  = p.HMACPrefix + licenseKey
// Data = date (DD/MM/YYYY UTC) + HWID
func (p product) token(date, hwid, licenseKey string) string {
	if p.HMACPrefix == "" {
		return ""
	}
	mac := hmac.New(sha256.New, []byte(p.HMACPrefix+licenseKey))
	mac.Write([]byte(date + hwid))
	return base64.StdEncoding.EncodeToString(mac.Sum(nil))
}

// ── Wire types ────────────────────────────────────────────────────────────────

// request mirrors the JSON body the DLL POSTs to /receiveCommand.
type request struct {
	Command    string `json:"command"`
	ProductID  string `json:"product_id"`
	Version    string `json:"version"`
	HWID       string `json:"HWID"`
	SID        string `json:"SID"`
	LicenseKey string `json:"license_key"`
	Hash       string `json:"hash"`
	Feedback   string `json:"feedback"`
}

// response mirrors every field the DLL may read from the server reply.
// omitempty keeps the JSON compact for fields the command doesn't need.
type response struct {
	Success              bool    `json:"success"`
	Message              string  `json:"message,omitempty"`
	URL                  string  `json:"url,omitempty"`
	URLName              string  `json:"url_name,omitempty"`
	WaitWarn             bool    `json:"wait_warn,omitempty"`
	WaitTime             int     `json:"wait_time,omitempty"`
	DownloadMessage      string  `json:"download_message,omitempty"`
	Variant              string  `json:"variant,omitempty"`  // ADO reads "variant" for current/latest version
	Version              string  `json:"version,omitempty"`  // CE  reads "version" for current/latest version
	ChangelogLink        string  `json:"changelog_link,omitempty"`
	Announcement         string  `json:"announcement,omitempty"`
	AnnouncementLink     string  `json:"announcement_link,omitempty"`
	AnnouncementLinkName string  `json:"announcement_link_name,omitempty"`
	TransferEmail        string  `json:"transfer_email,omitempty"`
	// verifylicense / activatelicense cache fields (written into DSLICINF so
	// the DLL skips the server on subsequent same-day startups).
	Date     string `json:"date,omitempty"`     // DD/MM/YYYY UTC — DLL compares to local clock
	Username string `json:"username,omitempty"` // display name stored in cache
	Role     string `json:"token,omitempty"`    // HMAC token validated by QueryServer / ViewProperty
	HWID     string `json:"m,omitempty"`        // hardware fingerprint echoed back from request
}

// ── HTTP handler ──────────────────────────────────────────────────────────────

func handleReceiveCommand(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
		return
	}

	body, err := io.ReadAll(io.LimitReader(r.Body, 1<<16))
	if err != nil {
		http.Error(w, "read error", http.StatusBadRequest)
		return
	}
	defer r.Body.Close()

	var req request
	if err := json.Unmarshal(body, &req); err != nil {
		log.Printf("  bad JSON from %s: %v", r.RemoteAddr, err)
		http.Error(w, "invalid json", http.StatusBadRequest)
		return
	}

	prod := productByID(req.ProductID)
	hwid := req.HWID
	if len(hwid) > 20 {
		hwid = hwid[:20] + "…"
	}
	log.Printf("→ cmd=%-24s product=%-16s ver=%-8s hwid=%s",
		req.Command, prod.Name, req.Version, hwid)

	resp := buildResponse(req, prod)
	log.Printf("← success=%-5v  message=%q", resp.Success, resp.Message)

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(resp)
}

// ── Command dispatch ──────────────────────────────────────────────────────────

func buildResponse(req request, prod product) response {
	switch strings.ToLower(req.Command) {
	case "activatelicense":
		return handleActivate(req, prod)
	case "verifylicense":
		return handleVerify(req, prod)
	case "getdownloadinfo":
		return handleDownloadInfo(req)
	case "sendfeedback":
		return handleFeedback(req)
	case "findsolution":
		return handleFindSolution(req)
	case "reportbug":
		return handleReportBug(req)
	case "transferlicenserequest", "transferlicense", "transferlicenseconfirm":
		return handleTransfer(req)
	default:
		log.Printf("  [unrecognised command %q — returning success]", req.Command)
		return response{Success: true}
	}
}

// ── Per-command handlers ──────────────────────────────────────────────────────

// handleActivate grants the license and populates DSLICINF cache fields.
// No message is needed on activate — the DLL shows a dialog for non-empty
// messages and would repeat it on every startup after cache expiry.
func handleActivate(req request, prod product) response {
	log.Printf("  [activated %s]", prod.Name)
	return licenseGranted(req, prod)
}

// handleVerify silently re-grants the license (no dialog on verify).
func handleVerify(req request, prod product) response {
	return licenseGranted(req, prod)
}

// handleDownloadInfo returns a no-update stub.
// Both "variant" (ADO) and "version" (CE) are echoed back as the current
// version so the semver comparison (current < latest) is always false.
// An empty/missing value crashes the semver constructor with NullReferenceException.
func handleDownloadInfo(req request) response {
	version := req.Version
	if version == "" {
		version = "0.0.0"
	}
	return response{
		Success:         true,
		DownloadMessage: "You are running the latest version.",
		Variant:         version, // ADO reads "variant"
		Version:         version, // CE  reads "version"
	}
}

func handleFeedback(req request) response {
	return response{Success: true, Message: "got feedback: " + req.Feedback}
}

func handleFindSolution(_ request) response {
	// Original server did a DB lookup; return success with no message to suppress the dialog.
	return response{Success: true}
}

func handleReportBug(_ request) response {
	return response{Success: true, Message: "Bug report received."}
}

func handleTransfer(_ request) response {
	return response{
		Success: true,
		Message: "License transfers are no longer necessary — the backend is offline and " +
			"this product has been restored for offline use.\nYou can run it on any machine.",
	}
}

// ── Helpers ───────────────────────────────────────────────────────────────────

// licenseGranted returns a successful verifylicense/activatelicense response
// with all DSLICINF cache fields populated so the DLL skips the server on
// subsequent same-day startups (TTL: 1 UTC day).
func licenseGranted(req request, prod product) response {
	date := time.Now().UTC().Format("02/01/2006")
	return response{
		Success:  true,
		Message:  "License verified.",
		Date:     date,
		Username: "Licensed User",
		Variant:  prod.Name,
		Role:     prod.token(date, req.HWID, req.LicenseKey),
		HWID:     req.HWID,
	}
}

