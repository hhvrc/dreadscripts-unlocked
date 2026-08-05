#!/usr/bin/env python3
"""
explore_dreadscripts.py
-----------------------
Fetches all remaining web-accessible DreadScripts assets and saves them to
dreadrith_exploration.zip.

No external dependencies — uses only the Python standard library.

Usage:
    python explore_dreadscripts.py

Output:
    dreadrith_exploration.zip   (extract into webdump/ when done)
    dreadrith_exploration_manifest.json
"""

import argparse
import json
import shutil
import sys
import tempfile
import time
import zipfile
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

sys.path.insert(0, str(Path(__file__).resolve().parent))
from pipeline import configure_console  # noqa: E402

# The report below is full of em dashes and rules; a Windows console is cp1252 and would raise
# UnicodeEncodeError on the first one, after every fetch had already been made.
configure_console()

# ── Constants ──────────────────────────────────────────────────────────────────

GCF_URL    = "https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand"
FB_BASE    = "https://dreadscripts-c6b62.web.app"
GCS_BASE   = "https://storage.googleapis.com"
GCS_BUCKET = "dreadscripts-c6b62.appspot.com"
GCS_API    = "https://www.googleapis.com/storage/v1"
GH_RAW     = "https://raw.githubusercontent.com/Dreadrith/DreadScripts/main"
GH_API     = "https://api.github.com"

UA = "Mozilla/5.0 (compatible; dreadrith-re/1.0)"

ZIP_NAME      = "dreadrith_exploration.zip"
MANIFEST_NAME = "dreadrith_exploration_manifest.json"

# ── Helpers ────────────────────────────────────────────────────────────────────

def fetch(url, method="GET", body=None, extra_headers=None, timeout=20):
    """
    Perform an HTTP request and return (status, headers_dict, body_bytes).
    Never raises — errors are returned as status=-1 with an error message in body.
    """
    headers = {"User-Agent": UA, "Accept": "*/*"}
    if body is not None:
        headers["Content-Type"] = "application/json"
    if extra_headers:
        headers.update(extra_headers)

    data = body.encode() if isinstance(body, str) else body
    req  = Request(url, data=data, headers=headers, method=method)

    try:
        with urlopen(req, timeout=timeout) as resp:
            return resp.status, dict(resp.headers), resp.read()
    except HTTPError as e:
        try:
            body_bytes = e.read()
        except Exception:
            body_bytes = b""
        return e.code, dict(e.headers), body_bytes
    except URLError as e:
        return -1, {}, str(e).encode()
    except Exception as e:
        return -1, {}, str(e).encode()


def headers_to_text(status, headers, url, method="GET", note=""):
    lines = [f"{method} {url}"]
    if note:
        lines.append(f"# {note}")
    lines.append(f"HTTP {status}")
    for k, v in headers.items():
        lines.append(f"{k}: {v}")
    return "\n".join(lines) + "\n"


def add(zf, path, data, manifest, url, status, note=""):
    """Write bytes into the zip and record in manifest."""
    if isinstance(data, str):
        data = data.encode()
    zf.writestr(path, data)
    manifest.append({
        "path":   path,
        "url":    url,
        "status": status,
        "size":   len(data),
        "note":   note,
    })
    tag = "OK " if status == 200 else f"{status} " if status > 0 else "ERR"
    print(f"  [{tag}] {path}  ({len(data)} B)")


def add_pair(zf, base_path, url, method, status, headers, body, manifest, note=""):
    """Save both a _headers.txt and a _body file for one request."""
    content_type = next((v for k, v in headers.items() if k.lower() == "content-type"), "")
    ext    = ".json" if "json" in content_type else ".txt"
    h_path = base_path + "_headers.txt"
    b_path = base_path + "_body" + ext
    add(zf, h_path, headers_to_text(status, headers, url, method, note), manifest, url, status, note + " [headers]")
    add(zf, b_path, body, manifest, url, status, note + " [body]")


# ── Section runners ────────────────────────────────────────────────────────────

def run_gcf_commands(zf, manifest):
    """Test every known DRM command against the GCF endpoint."""
    print("\n── GCF DRM commands ─────────────────────────────────────────────")

    # Shared dummy values — we're just confirming all commands return the same
    # static shutdown response regardless of payload content.
    dummy_ado = {
        "product_id":  "No1lKII9IzcBAbihub6nCg==LK",
        "version":     "0.11.1",
        "HWID":        "PROBE-HWID-001-002-003",
        "SID":         "deadbeefdeadbeefdeadbeefdeadbeef",
        "license_key": "AAAAAAAA-BBBBBBBB-CCCCCCCC-DDDDDDDD",
        "hash":        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
    }
    dummy_ce = dict(dummy_ado, product_id="yOk0XCnENLMO6DIF8cYpSg==LK", version="3.3.2")

    commands = [
        # (slug, product_variant, extra_fields)
        ("activatelicense",      "ado", {}),
        ("activatelicense",      "ce",  {}),
        ("verifylicense",        "ado", {}),
        ("verifylicense",        "ce",  {}),
        ("getdownloadinfo",      "ado", {}),
        ("getdownloadinfo",      "ce",  {}),
        ("transferlicenserequest", "ce", {}),
        ("sendfeedback",         "ce",  {"feedback": "probe test"}),
        ("findsolution",         "ce",  {"bug_id": "0", "bug_version": "3.3.2",
                                          "bug_name": "probe", "bug_exception": "probe"}),
        ("reportbug",            "ce",  {"bug_id": "0", "bug_version": "3.3.2",
                                          "bug_name": "probe", "bug_exception": "probe",
                                          "solution": "none"}),
    ]

    for cmd, prod, extras in commands:
        base     = dummy_ado if prod == "ado" else dummy_ce
        payload  = json.dumps({"command": cmd, **base, **extras})
        status, hdrs, body = fetch(GCF_URL, method="POST", body=payload)
        slug     = f"gcf/commands/{cmd}_{prod}"
        add_pair(zf, slug, GCF_URL, "POST", status, hdrs, body, manifest,
                 f"command={cmd} product={prod}")
        time.sleep(0.3)

    # OPTIONS — check CORS
    status, hdrs, body = fetch(GCF_URL, method="OPTIONS",
                                extra_headers={"Origin": "https://evil.example.com",
                                               "Access-Control-Request-Method": "POST"})
    add_pair(zf, "gcf/options_cors", GCF_URL, "OPTIONS", status, hdrs, body, manifest,
             "CORS preflight probe")


def run_firebase_vpm_probes(zf, manifest):
    """Probe for VPM listings not yet archived."""
    print("\n── Firebase Hosting — VPM listing probes ────────────────────────")

    unknown_packages = [
        "com.dreadscripts.avatardynamicsoverhaul",
        "com.dreadscripts.controllereditor",
        "com.dreadscripts.adoverhaul",
        "com.dreadscripts.avatar-dynamics-overhaul",
        "com.dreadscripts.physbone-overhaul",
        "com.dreadscripts.controller-editor",
    ]

    for pkg in unknown_packages:
        url    = f"{FB_BASE}/listings/{pkg}.json"
        status, hdrs, body = fetch(url)
        # Only save if it returns something other than the SPA catch-all
        is_spa = (status == 200 and len(body) == 6271)
        note   = "SPA catch-all (not found)" if is_spa else ""
        add(zf, f"firebase_hosting/listings/probe_{pkg}.json" if not is_spa else
                f"firebase_hosting/listings/probe_{pkg}_404.html",
            body, manifest, url, status, note or f"probe {pkg}")
        time.sleep(0.2)


def run_github(zf, manifest):
    """Fetch GitHub assets referenced by the DLLs."""
    print("\n── GitHub ───────────────────────────────────────────────────────")

    # Changelog
    url    = f"{GH_RAW}/ControllerEditor/Changelog.txt"
    status, hdrs, body = fetch(url)
    add(zf, "github/ControllerEditor_Changelog.txt", body, manifest, url, status)

    # DreadBanner image
    url    = f"{GH_RAW}/Other/DreadBanner.png"
    status, hdrs, body = fetch(url)
    add(zf, "github/DreadBanner.png", body, manifest, url, status,
        "DreadBanner referenced in DLL strings")

    # ADOverhaul changelog (speculative path)
    for path in ("ADOverhaul/Changelog.txt", "ADOverhaul/CHANGELOG.txt",
                 "Other/ADOverhaul/Changelog.txt"):
        url    = f"{GH_RAW}/{path}"
        status, hdrs, body = fetch(url)
        if status == 200:
            add(zf, f"github/ADOverhaul_Changelog.txt", body, manifest, url, status)
            break
        time.sleep(0.2)

    # GitHub API — public repos
    url    = f"{GH_API}/users/Dreadrith/repos?per_page=100&sort=updated"
    status, hdrs, body = fetch(url, extra_headers={"Accept": "application/vnd.github+json"})
    add(zf, "github/repos.json", body, manifest, url, status, "public repos list")

    # GitHub API — DreadScripts repo contents (look for more known asset paths)
    url    = f"{GH_API}/repos/Dreadrith/DreadScripts/contents"
    status, hdrs, body = fetch(url, extra_headers={"Accept": "application/vnd.github+json"})
    add(zf, "github/DreadScripts_repo_contents.json", body, manifest, url, status)

    time.sleep(0.3)


def run_supporter_assets(zf, manifest):
    """Fetch supporter window images referenced by both DLLs."""
    print("\n── Supporter assets (Imgur) ─────────────────────────────────────")

    assets = [
        ("https://i.imgur.com/iHszIY3.png", "supporter_assets/iHszIY3_banner.png",
         "SupportWindow banner (ds-supporters-main)"),
        ("https://i.imgur.com/FMv1R6A.png", "supporter_assets/FMv1R6A_avatar.png",
         "SupportWindow avatar (ds-supporters-kofi)"),
    ]
    for url, path, note in assets:
        status, hdrs, body = fetch(url)
        add(zf, path, body, manifest, url, status, note)
        time.sleep(0.3)


def run_gcs_probes(zf, manifest):
    """Probe GCS bucket for known and speculative file paths."""
    print("\n── Google Cloud Storage — bucket probes ─────────────────────────")

    # JSON API bucket listing (anonymous — expect 401/403)
    url    = f"{GCS_API}/b/{GCS_BUCKET}/o?maxResults=100"
    status, hdrs, body = fetch(url)
    add(zf, "gcs/bucket_listing_api.json", body, manifest, url, status,
        "GCS JSON API listing (expected 401)")

    # Direct public object URLs — known from DLL strings + Supporters.txt already archived
    known_objects = [
        ("Dreadscripts/Supporters.txt",         "gcs/Supporters.txt"),
    ]
    # Speculative paths
    speculative = [
        ("Dreadscripts/ADOverhaul/Changelog.txt",    "gcs/ADOverhaul_Changelog.txt"),
        ("Dreadscripts/ControllerEditor/Changelog.txt", "gcs/ControllerEditor_Changelog.txt"),
        ("ADOverhaul/Changelog.txt",                 "gcs/ADOverhaul_Changelog_alt.txt"),
    ]

    for obj_path, zip_path in known_objects + speculative:
        url    = f"{GCS_BASE}/{GCS_BUCKET}/{obj_path}"
        status, hdrs, body = fetch(url)
        note   = f"GCS object: {obj_path}"
        add(zf, zip_path, body, manifest, url, status, note)
        time.sleep(0.2)


def run_docs(zf, manifest):
    """Fetch documentation and social pages referenced by the DLLs."""
    print("\n── Docs & social pages ──────────────────────────────────────────")

    pages = [
        # (url, zip_path, note)
        ("https://notes.sleightly.dev/ceditor",
         "docs/notes_sleightly_ceditor.html",
         "ControllerEditor docs (referenced in DLL)"),
        ("https://notes.sleightly.dev/controllereditor/",
         "docs/notes_sleightly_controllereditor.html",
         "ControllerEditor docs alt URL"),
        ("https://notes.sleightly.dev/templates/",
         "docs/notes_sleightly_templates.html",
         "Templates docs (referenced in DLL)"),
        ("https://dreadrith.com/license-tos",
         "docs/dreadrith_license_tos.html",
         "License ToS (shown in transfer flow)"),
        ("https://dreadrith.com/links",
         "docs/dreadrith_links.html",
         "Links page (referenced in DLL)"),
        ("https://linktr.ee/Dreadrith",
         "docs/linktree_dreadrith.html",
         "Linktree (referenced in ADOverhaul DLL)"),
        ("https://ko-fi.com/dreadrith",
         "docs/kofi_dreadrith.html",
         "Ko-fi page (supporter window)"),
        ("https://vpm.dreadscripts.com/",
         "docs/vpm_dreadscripts_root.html",
         "VPM domain root (CNAME → Porkbun)"),
        ("https://vpm.dreadscripts.com/index.json",
         "docs/vpm_dreadscripts_index.json",
         "VPM index (speculative path)"),
    ]

    for url, zip_path, note in pages:
        status, hdrs, body = fetch(url, timeout=15)
        add(zf, zip_path, body, manifest, url, status, note)
        time.sleep(0.4)


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    # Argument parsing exists mainly so that `--help` cannot start a live archival run. It had no
    # parser at all, so every argument -- including --help -- fell through to the fetch loop, which
    # hits dozens of remote endpoints and OVERWRITES the tracked manifest with whatever they return
    # today. That is not a hypothetical: a --help typed to check the interface replaced the
    # 2026-07-30 manifest, and several recorded response sizes came back smaller, which is a finding
    # about the remote endpoints that should be made deliberately rather than by accident.
    ap = argparse.ArgumentParser(
        description="Fetch remaining web-accessible DreadScripts assets into "
                    f"{ZIP_NAME} and {MANIFEST_NAME}.")
    ap.add_argument("--dry-run", action="store_true",
                    help="list what would be fetched and exit without touching the network "
                         "or either output file")
    args = ap.parse_args()

    if args.dry_run:
        print(f"dry run -- would fetch into {ZIP_NAME} and rewrite {MANIFEST_NAME}")
        for stage in ("GCF DRM commands", "Firebase/VPM probes", "GitHub", "supporter assets",
                      "GCS probes", "docs"):
            print(f"  {stage}")
        print("\nThe manifest is tracked: a real run rewrites it, and a shrunken response size "
              "there is a finding, not noise.")
        return

    print(f"dreadrith explorer — {datetime.now(timezone.utc).isoformat()}")
    print(f"Output: {ZIP_NAME}\n")

    manifest  = []
    # Keep normal archival runs in memory but spill oversized remote responses to disk instead of
    # retaining both every body and the complete zip in RAM.
    archive = tempfile.SpooledTemporaryFile(max_size=8 * 1024 * 1024)

    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as zf:
        run_gcf_commands(zf, manifest)
        run_firebase_vpm_probes(zf, manifest)
        run_github(zf, manifest)
        run_supporter_assets(zf, manifest)
        run_gcs_probes(zf, manifest)
        run_docs(zf, manifest)

        # Write manifest inside the zip
        manifest_json = json.dumps({
            "generated":  datetime.now(timezone.utc).isoformat(),
            "tool":       "explore_dreadscripts.py",
            "entries":    manifest,
            "summary": {
                "total":   len(manifest),
                "ok_200":  sum(1 for e in manifest if e["status"] == 200),
                "errors":  sum(1 for e in manifest if e["status"] <= 0),
                "other":   sum(1 for e in manifest if 0 < e["status"] != 200),
            },
        }, indent=2)
        zf.writestr("manifest.json", manifest_json)

    archive_size = archive.tell()
    archive.seek(0)
    with open(ZIP_NAME, "wb") as output:
        shutil.copyfileobj(archive, output)
    archive.close()

    # Also write manifest alongside the zip for quick inspection
    with open(MANIFEST_NAME, "w", encoding="utf-8") as f:
        f.write(manifest_json)

    ok    = sum(1 for e in manifest if e["status"] == 200)
    total = len(manifest)
    print(f"\nDone — {ok}/{total} fetches returned 200.")
    print(f"Zip:      {ZIP_NAME}  ({archive_size:,} B)")
    print(f"Manifest: {MANIFEST_NAME}")
    print()
    print("Extract the zip into webdump/ and update INDEX.md with new findings.")
    print("Suggested layout additions:")
    print("  webdump/gcf/commands/")
    print("  webdump/github/")
    print("  webdump/supporter_assets/")
    print("  webdump/gcs/")
    print("  webdump/docs/")
    print("  webdump/firebase_hosting/listings/  (probes)")


if __name__ == "__main__":
    main()
