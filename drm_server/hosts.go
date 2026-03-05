package main

import (
	"fmt"
	"log"
	"os"
	"strings"
)

const hostsPath  = `C:\Windows\System32\drivers\etc\hosts`
const hostsEntry = "127.0.0.1  " + drmHost

// patchHosts appends a redirect entry for the DRM hostname to the system hosts
// file if one is not already present.  Requires Administrator privileges.
func patchHosts() error {
	data, err := os.ReadFile(hostsPath)
	if err != nil {
		return fmt.Errorf("read hosts file: %w (run as Administrator?)", err)
	}
	if strings.Contains(string(data), drmHost) {
		log.Printf("hosts: entry already present — no change needed")
		return nil
	}
	updated := append(data, []byte("\n"+hostsEntry+"\n")...)
	if err := os.WriteFile(hostsPath, updated, 0644); err != nil {
		return fmt.Errorf("write hosts file: %w (run as Administrator?)", err)
	}
	log.Printf("hosts: added → %s", hostsEntry)
	return nil
}
