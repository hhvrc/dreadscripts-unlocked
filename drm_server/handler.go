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
	"net/url"
	"regexp"
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

// transferCodePattern is the DLL's own client-side rule for the transfer
// verification code (six alphanumerics; its text field additionally strips
// everything but digits before enabling the button). Anything that reaches us
// from a real plugin already satisfies this.
var transferCodePattern = regexp.MustCompile(`^[a-zA-Z0-9]{6}$`)

// ── Wire types ────────────────────────────────────────────────────────────────

// request mirrors the JSON body the DLL POSTs to /receiveCommand.
//
// The DLL builds the body by hand as `"key":"value"` pairs, so every value on
// the wire is a JSON string — none of these are numbers or bools even when the
// underlying field is one.
type request struct {
	Command    string `json:"command"`
	ProductID  string `json:"product_id"`
	Version    string `json:"version"`
	HWID       string `json:"HWID"`
	SID        string `json:"SID"`
	LicenseKey string `json:"license_key"`
	// hash = base64(HMACSHA256(key = product secret, data = every value above
	// concatenated in request order)). Appended to every command except
	// getdownloadinfo, which posts only command/product_id/version and no hash.
	// Not checked here: it authenticates the client to the server, and this
	// server has nothing to withhold from a client that fails it.
	Hash string `json:"hash"`
	// sendfeedback / reportbug: free text, Uri.EscapeUriString'd by the DLL and
	// truncated to 2000 chars before sending.
	Feedback string `json:"feedback"`
	// transferlicenseconfirm: the 6-digit code the real backend would have
	// e-mailed. Dropped silently before this field existed.
	VerificationCode string `json:"verification_code"`
	// findsolution / reportbug: the caught exception the DLL is asking about.
	BugID        string `json:"bug_id"`        // BugReporter.ErrorInfo.id, a ushort
	BugVersion   string `json:"bug_version"`   // BugReporter.ErrorInfo.version, a ushort
	BugName      string `json:"bug_name"`      // name of the feature that threw
	BugException string `json:"bug_exception"` // exception message, Uri.EscapeUriString'd on findsolution
}

// response mirrors every field the DLL actually reads from the server reply.
// The full read set was taken from every JsonObject.Item("…") call site in
// ADOverhaul 2019/2022 and ControllerEditor; there are no dynamic key lookups,
// so anything not listed here is ignored by the plugins.
type response struct {
	Success bool `json:"success"`
	// message is emitted even when empty rather than omitted. ADOverhaul2022's
	// decompiled findsolution callback reaches Log(message) on the blank-message
	// branch with no null guard, which would throw on an absent key; ADOverhaul2019
	// and ControllerEditor both guard it, so the 2022 shape is most likely a
	// decompilation artefact — but an explicit "" is safe under either reading.
	Message string `json:"message"`

	// Failure-path fields. The shared response handler only looks at these when
	// success is false, where it raises a dialog and rate-limits the buttons.
	URL      string `json:"url,omitempty"`       // gives the dialog a second button that opens this
	URLName  string `json:"url_name,omitempty"`  // label for it; the DLL defaults to "Link"
	WaitWarn bool   `json:"wait_warn,omitempty"` // latches "further failed attempts will get your device blocked"
	WaitTime int    `json:"wait_time,omitempty"` // seconds to disable the retry buttons for

	// verifylicense / activatelicense success path. These are also written into
	// the DLL's encrypted SessionState cache so it skips the server on later
	// same-day startups (TTL: one UTC day).
	Date     string `json:"date,omitempty"`     // DD/MM/YYYY UTC — must equal the DLL's own UTC date or it aborts
	Username string `json:"username,omitempty"` // shown as "Authorized For: <username>"
	Variant  string `json:"variant,omitempty"`  // licence tier, shown as "License: <variant>"; display-only
	Token    string `json:"token,omitempty"`    // HMAC-SHA256 gate — a success without this restores nothing

	// transferlicenserequest success path: the address the DLL tells the user
	// the 6-digit code was mailed to.
	TransferEmail string `json:"transfer_email,omitempty"`

	// findsolution success path.
	Solution         string `json:"solution,omitempty"` // known-issue text; blank means "No solution Found!"
	SolutionComplete bool   `json:"complete,omitempty"` // true → "Solution Found!", false → "Known issue! Details:"

	// getdownloadinfo. This callback bypasses the shared response handler and
	// reads these directly, so success/message are ignored for this command.
	Version              string `json:"version,omitempty"`       // latest version "a.b.c"; each part goes through int.Parse
	DownloadLink         string `json:"download_link,omitempty"` // .unitypackage URL for the "Download Update" button
	DownloadMessage      string `json:"download_message,omitempty"`
	ChangelogLink        string `json:"changelog_link,omitempty"`
	Announcement         string `json:"announcement,omitempty"`
	AnnouncementLink     string `json:"announcement_link,omitempty"`
	AnnouncementLinkName string `json:"announcement_link_name,omitempty"`
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

// buildResponse routes one request. These eight commands are the complete set
// the plugins send to this host — every call site builds its command string as
// a literal, so the list is exhaustive.
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
	case "transferlicenserequest":
		return handleTransferRequest(req)
	case "transferlicenseconfirm":
		return handleTransferConfirm(req)
	default:
		log.Printf("  [unrecognised command %q — returning success]", req.Command)
		return response{Success: true}
	}
}

// ── Per-command handlers ──────────────────────────────────────────────────────

// handleActivate grants the license and populates DSLICINF cache fields.
// NOTE: licenseGranted() always sets message="License verified." — if the DLL
// shows a dialog for any non-empty message and this ends up repeating on every
// startup after cache expiry, this handler (not handleVerify) needs to drop the
// message; not changed here without confirming actual DLL dialog behavior first.
func handleActivate(req request, prod product) response {
	log.Printf("  [activated %s]", prod.Name)
	return licenseGranted(req, prod)
}

// handleVerify re-grants the license the same way handleActivate does — same
// message, same cache fields, no separate "silent" behavior currently exists.
func handleVerify(req request, prod product) response {
	return licenseGranted(req, prod)
}

// handleDownloadInfo returns a no-update stub.
// "version" is the field both ADOverhaul and ControllerEditor compare against
// their own version, so echoing the caller's version back makes
// (current < latest) false and the update banner stays hidden.
// It must be present and parseable: the DLL feeds it straight into a semver
// constructor that int.Parses three dot-separated parts, and an absent key
// arrives as null there. The DLL also re-requests on every domain reload while
// its cached version string is blank, so an empty reply would never settle.
//
// download_link is deliberately left empty — with no update on offer there is
// nothing to download, and a blank link hides the "Download Update" button.
// The announcement_* fields are likewise absent, which the DLL reads as
// "no announcement" and skips the banner.
func handleDownloadInfo(req request) response {
	version := req.Version
	if version == "" {
		version = "0.0.0"
	}
	return response{
		Success:         true,
		DownloadMessage: "You are running the latest version.",
		Version:         version,
	}
}

// handleFeedback acknowledges feedback. The DLL routes this through the shared
// response handler, so a non-empty message is echoed into the Unity console as
// the user's confirmation that it went through.
func handleFeedback(req request) response {
	log.Printf("  [feedback] %s", unescape(req.Feedback))
	return response{Success: true, Message: "Thanks! Your feedback was received."}
}

// handleFindSolution answers the known-issue lookup the bug reporter performs
// before it lets the user type a report. The original server did a DB lookup;
// with no database, "solution" is left blank, which is exactly the reply that
// puts the DLL on its "No solution Found!" branch and opens the report box.
// The message is left empty so nothing is logged for a lookup that found
// nothing — see the note on response.Message for why it is still emitted.
func handleFindSolution(req request) response {
	log.Printf("  [findsolution] id=%s ver=%s name=%q exception=%q",
		req.BugID, req.BugVersion, req.BugName, unescape(req.BugException))
	return response{Success: true}
}

// handleReportBug accepts the report the user typed after findsolution came up
// empty. The message is shown in the Unity console as the send confirmation.
// Note the DLL escapes bug_exception on findsolution but sends it raw here.
func handleReportBug(req request) response {
	log.Printf("  [reportbug] id=%s ver=%s name=%q exception=%q report=%q",
		req.BugID, req.BugVersion, req.BugName, req.BugException, unescape(req.Feedback))
	return response{Success: true, Message: "Bug report received."}
}

// handleTransferRequest answers step one of the transfer flow. On success the
// DLL stores "transfer_email", flips its panel to the code-entry stage and
// prints "A 6-digit verification code was sent to <transfer_email>." — so that
// field has to be present or the user is told the code went to nowhere.
// Nothing is actually mailed: this server grants every license anyway, and the
// message says so before the user goes looking for an e-mail.
func handleTransferRequest(_ request) response {
	return response{
		Success:       true,
		TransferEmail: "(no one — this is a local restoration server)",
		Message: "No verification e-mail was sent — the backend is offline and this " +
			"product has been restored for offline use.\nEnter any 6 digits to continue.",
	}
}

// handleTransferConfirm answers step two. Success closes the transfer panel and
// makes the DLL re-run verifylicense, which grants the license again on the new
// device, so nothing has to be moved server-side.
// The code is checked only against the DLL's own client-side rule: its button
// stays disabled unless the field matches [0-9]{6}, so a real plugin can never
// fail this, and a request that does fail it did not come from one.
func handleTransferConfirm(req request) response {
	if !transferCodePattern.MatchString(req.VerificationCode) {
		log.Printf("  [transfer rejected: verification_code %q is not 6 alphanumerics]", req.VerificationCode)
		return response{
			Success: false,
			Message: "That verification code is not valid. Enter any 6 digits — " +
				"this server does not send codes.",
		}
	}
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
//
// "variant" is the licence tier, not the product — the DLL only ever renders it
// as "License: <variant>" and falls back to "Personal" when it is blank, so
// that is what is sent. It is not left empty because the DLL then feeds a null
// through to SessionState.SetString when it writes the cache.
func licenseGranted(req request, prod product) response {
	date := time.Now().UTC().Format("02/01/2006")
	return response{
		Success:  true,
		Message:  "License verified.",
		Date:     date,
		Username: "Licensed User",
		Variant:  "Personal",
		Token:    prod.token(date, req.HWID, req.LicenseKey),
	}
}

// unescape reverses the DLL's Uri.EscapeUriString for logging. Path (not query)
// unescaping is the right inverse: EscapeUriString percent-encodes spaces
// rather than turning them into '+'. Undecodable input is logged verbatim.
func unescape(s string) string {
	if decoded, err := url.PathUnescape(s); err == nil {
		return decoded
	}
	return s
}
