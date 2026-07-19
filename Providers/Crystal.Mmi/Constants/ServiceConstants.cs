using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class ServiceConstants {
  public const string QueryString = "SELECT * FROM Win32_Service";

  public const string AcceptPauseKey = "AcceptPause";
  public const string AcceptPauseDesc = "If True, the service can be paused";

  public const string AcceptStopKey = "AcceptStop";
  public const string AcceptStopDesc = "If True, the service can be stopped";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the service";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class to appear in the inheritance chain";

  public const string DelayedAutoStartKey = "DelayedAutoStart";
  public const string DelayedAutoStartDesc = "If True, the service is started after other auto-start services plus a short delay";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the service";

  public const string DesktopInteractKey = "DesktopInteract";
  public const string DesktopInteractDesc = "If True, the service can interact with the desktop";

  public const string DisplayNameKey = "DisplayName";
  public const string DisplayNameDesc = "Display name of the service, as shown in administrative tools";

  public const string ErrorControlKey = "ErrorControl";
  public const string ErrorControlDesc = "Severity of the error if the service fails to start during boot";

  public const string ExitCodeKey = "ExitCode";
  public const string ExitCodeDesc = "Win32 error code the service uses to report an error at startup or shutdown";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the service was installed";

  public const string NameKey = "Name";
  public const string NameDesc = "Unique identifier of the service; the name used internally by the system";

  public const string PathNameKey = "PathName";
  public const string PathNameDesc = "Fully-qualified path to the service's executable file";

  public const string ProcessIdKey = "ProcessId";
  public const string ProcessIdDesc = "Process identifier (PID) of the process hosting the service";

  public const string ServiceSpecificExitCodeKey = "ServiceSpecificExitCode";
  public const string ServiceSpecificExitCodeDesc = "Service-specific error code the service reports when using ExitCode of ERROR_SERVICE_SPECIFIC_ERROR";

  public const string ServiceTypeKey = "ServiceType";
  public const string ServiceTypeDesc = "Type of service, e.g. Win32 Own Process, Win32 Share Process, Kernel Driver";

  public const string StartedKey = "Started";
  public const string StartedDesc = "If True, the service has been started";

  public const string StartModeKey = "StartMode";
  public const string StartModeDesc = "Start mode of the service (Boot, System, Automatic, Manual, Disabled)";

  public const string StartNameKey = "StartName";
  public const string StartNameDesc = "Account name under which the service runs";

  public const string StateKey = "State";
  public const string StateDesc = "Current state of the service (Running, Stopped, Paused, etc.)";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the service object";

  public const string SystemCreationClassNameKey = "SystemCreationClassName";
  public const string SystemCreationClassNameDesc = "Value of the scoping computer system's CreationClassName property";

  public const string SystemNameKey = "SystemName";
  public const string SystemNameDesc = "Name of the scoping system";

  public const string TagIdKey = "TagId";
  public const string TagIdDesc = "Unique tag value used to order service startup within a load order group";

  public const string WaitHintKey = "WaitHint";
  public const string WaitHintDesc = "Estimated time, in milliseconds, required for a pending start/stop/pause/continue operation";
}
