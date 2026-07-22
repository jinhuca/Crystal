using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.Thread;

internal static class WmiThread {
  public const string ClassName = WmiClasses.Thread;

  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;

  public const string CreationClassName = nameof(CreationClassName);
  public const string ElapsedTime = nameof(ElapsedTime);
  public const string ExecutionState = nameof(ExecutionState);
  public const string Handle = nameof(Handle);
  public const string InstallationDate = nameof(InstallationDate);
  public const string KernelModeTime = nameof(KernelModeTime);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string Priority = nameof(Priority);
  public const string ProcessCreationClassName = nameof(ProcessCreationClassName);
  public const string ProcessHandle = nameof(ProcessHandle);
  public const string StartAddress = nameof(StartAddress);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string ThreadState = nameof(ThreadState);
  public const string ThreadWaitReason = nameof(ThreadWaitReason);
  public const string UserModeTime = nameof(UserModeTime);
}
