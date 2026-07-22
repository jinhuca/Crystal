using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.Process;

internal static class WmiProcess {
  public const string ClassName = WmiClasses.Process;

  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;

  public const string CreationClassName = nameof(CreationClassName);
  public const string ExecutionState = nameof(ExecutionState);
  public const string Handle = nameof(Handle);
  public const string HandleCount = nameof(HandleCount);
  public const string InstallationDate = nameof(InstallationDate);
  public const string KernelModeTime = nameof(KernelModeTime);
  public const string PageFileUsage = nameof(PageFileUsage);
  public const string ParentProcessId = nameof(ParentProcessId);
  public const string PeakPageFileUsage = nameof(PeakPageFileUsage);
  public const string PeakVirtualSize = nameof(PeakVirtualSize);
  public const string PeakWorkingSetSize = nameof(PeakWorkingSetSize);
  public const string Priority = nameof(Priority);
  public const string ProcessId = nameof(ProcessId);
  public const string ThreadCount = nameof(ThreadCount);
  public const string UserModeTime = nameof(UserModeTime);
  public const string VirtualSize = nameof(VirtualSize);
  public const string WorkingSetSize = nameof(WorkingSetSize);
  public const string CommandLine = nameof(CommandLine);
  public const string CreationDate = nameof(CreationDate);
  public const string ExecutablePath = nameof(ExecutablePath);
  public const string MaximumWorkingSetSize = nameof(MaximumWorkingSetSize);
  public const string MinimumWorkingSetSize = nameof(MinimumWorkingSetSize);
  public const string OtherOperationCount = nameof(OtherOperationCount);
  public const string OtherTransferCount = nameof(OtherTransferCount);
  public const string PageFaults = nameof(PageFaults);
  public const string ReadOperationCount = nameof(ReadOperationCount);
  public const string ReadTransferCount = nameof(ReadTransferCount);
  public const string SessionId = nameof(SessionId);
  public const string TerminationDate = nameof(TerminationDate);
  public const string WindowsVersion = nameof(WindowsVersion);
  public const string WriteOperationCount = nameof(WriteOperationCount);
  public const string WriteTransferCount = nameof(WriteTransferCount);
  public const string PrivatePageCount = nameof(PrivatePageCount);
}