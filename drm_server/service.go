package main

import (
	"fmt"
	"log"
	"os"
	"path/filepath"
	"time"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/eventlog"
	"golang.org/x/sys/windows/svc/mgr"
)

const serviceName = "DreadScriptsDRM"
const serviceDesc = "DreadScripts local DRM bypass — spoofs the Firebase license endpoint for ADOverhaul and ControllerEditor"

// drmService implements svc.Handler.
type drmService struct{}

func (s *drmService) Execute(args []string, r <-chan svc.ChangeRequest, changes chan<- svc.Status) (svcSpecificEC bool, errno uint32) {
	changes <- svc.Status{State: svc.StartPending}

	// Start the HTTPS server in a goroutine.
	errCh := make(chan error, 1)
	go func() {
		errCh <- runServe(":443")
	}()

	changes <- svc.Status{State: svc.Running, Accepts: svc.AcceptStop | svc.AcceptShutdown}

	for {
		select {
		case err := <-errCh:
			log.Printf("server error: %v", err)
			return false, 1
		case c := <-r:
			switch c.Cmd {
			case svc.Stop, svc.Shutdown:
				changes <- svc.Status{State: svc.StopPending}
				// runServe blocks on srv.Serve; no clean shutdown handle, but SCM
				// will kill the process after StopPending timeout (~5 s).
				return false, 0
			}
		}
	}
}

// runAsService is called when the process is started by the Windows SCM.
func runAsService() error {
	elog, err := eventlog.Open(serviceName)
	if err == nil {
		defer elog.Close()
		// Redirect the default logger to the Windows event log.
		log.SetOutput(&eventLogWriter{elog})
	}
	return svc.Run(serviceName, &drmService{})
}

// eventLogWriter satisfies io.Writer by writing to the Windows event log.
type eventLogWriter struct{ el *eventlog.Log }

func (w *eventLogWriter) Write(p []byte) (int, error) {
	_ = w.el.Info(1, string(p))
	return len(p), nil
}

// installService registers the service with the SCM and sets it to auto-start.
func installService() error {
	exePath, err := os.Executable()
	if err != nil {
		return fmt.Errorf("get executable path: %w", err)
	}
	exePath, err = filepath.Abs(exePath)
	if err != nil {
		return fmt.Errorf("abs path: %w", err)
	}

	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("connect to SCM: %w (run as Administrator?)", err)
	}
	defer m.Disconnect()

	// Check if it already exists.
	s, err := m.OpenService(serviceName)
	if err == nil {
		s.Close()
		return fmt.Errorf("service %q already exists — run uninstall-service first", serviceName)
	}

	s, err = m.CreateService(serviceName, exePath, mgr.Config{
		DisplayName:  "DreadScripts DRM Server",
		Description:  serviceDesc,
		StartType:    mgr.StartAutomatic,
		ErrorControl: mgr.ErrorNormal,
	}, "serve") // pass "serve" so the SCM-launched binary takes the serve path
	if err != nil {
		return fmt.Errorf("create service: %w", err)
	}
	defer s.Close()

	// Register an event source so Windows Event Viewer can show our logs.
	if err := eventlog.InstallAsEventCreate(serviceName, eventlog.Error|eventlog.Warning|eventlog.Info); err != nil {
		log.Printf("warning: could not register event source: %v", err)
	}

	log.Printf("Service %q installed (auto-start).", serviceName)
	log.Printf("Start it now with:  sc start %s", serviceName)
	return nil
}

// uninstallService stops and removes the service from the SCM.
func uninstallService() error {
	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("connect to SCM: %w (run as Administrator?)", err)
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return fmt.Errorf("service %q not found: %w", serviceName, err)
	}
	defer s.Close()

	// Attempt a graceful stop first.
	status, err := s.Control(svc.Stop)
	if err == nil {
		deadline := time.Now().Add(10 * time.Second)
		for status.State != svc.Stopped && time.Now().Before(deadline) {
			time.Sleep(300 * time.Millisecond)
			status, _ = s.Query()
		}
	}

	if err := s.Delete(); err != nil {
		return fmt.Errorf("delete service: %w", err)
	}

	_ = eventlog.Remove(serviceName)
	log.Printf("Service %q uninstalled.", serviceName)
	return nil
}

// startService asks the SCM to start the service.
func startService() error {
	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("connect to SCM: %w (run as Administrator?)", err)
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return fmt.Errorf("service %q not found — run install-service first", serviceName)
	}
	defer s.Close()

	if err := s.Start("serve"); err != nil {
		return fmt.Errorf("start service: %w", err)
	}
	log.Printf("Service %q started.", serviceName)
	return nil
}

// stopService asks the SCM to stop the service.
func stopService() error {
	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("connect to SCM: %w (run as Administrator?)", err)
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return fmt.Errorf("service %q not found", serviceName)
	}
	defer s.Close()

	if _, err := s.Control(svc.Stop); err != nil {
		return fmt.Errorf("stop service: %w", err)
	}
	log.Printf("Service %q stop signal sent.", serviceName)
	return nil
}
