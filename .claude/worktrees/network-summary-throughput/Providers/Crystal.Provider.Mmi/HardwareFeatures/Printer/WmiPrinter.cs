using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.Printer;

internal static class WmiPrinter {
  public const string ClassName = WmiClasses.Printer;

  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  public const string Attributes = nameof(Attributes);
  public const string Availability = nameof(Availability);
  public const string AveragePagesPerMinute = nameof(AveragePagesPerMinute);
  public const string Comment = nameof(Comment);
  public const string Default = nameof(Default);
  public const string DefaultPriority = nameof(DefaultPriority);
  public const string DetectedErrorState = nameof(DetectedErrorState);
  public const string Direct = nameof(Direct);
  public const string DoCompleteFirst = nameof(DoCompleteFirst);
  public const string DriverName = nameof(DriverName);
  public const string EnableBIDI = nameof(EnableBIDI);
  public const string EnableDevQueryPrint = nameof(EnableDevQueryPrint);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ExtendedDetectedErrorState = nameof(ExtendedDetectedErrorState);
  public const string ExtendedPrinterStatus = nameof(ExtendedPrinterStatus);
  public const string Hidden = nameof(Hidden);
  public const string HorizontalResolution = nameof(HorizontalResolution);
  public const string JobCountSinceLastReset = nameof(JobCountSinceLastReset);
  public const string KeepPrintedJobs = nameof(KeepPrintedJobs);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string Local = nameof(Local);
  public const string Location = nameof(Location);
  public const string MaxCopies = nameof(MaxCopies);
  public const string MaxNumberUp = nameof(MaxNumberUp);
  public const string MaxSizeSupported = nameof(MaxSizeSupported);
  public const string Network = nameof(Network);
  public const string PaperSizesSupported = nameof(PaperSizesSupported);
  public const string PortName = nameof(PortName);
  public const string PrinterPaperNames = nameof(PrinterPaperNames);
  public const string PrinterState = nameof(PrinterState);
  public const string PrinterStatus = nameof(PrinterStatus);
  public const string PrintJobDataType = nameof(PrintJobDataType);
  public const string PrintProcessor = nameof(PrintProcessor);
  public const string Priority = nameof(Priority);
  public const string Published = nameof(Published);
  public const string Queued = nameof(Queued);
  public const string RawOnly = nameof(RawOnly);
  public const string SeparatorFile = nameof(SeparatorFile);
  public const string ServerName = nameof(ServerName);
  public const string ShareName = nameof(ShareName);
  public const string Shared = nameof(Shared);
  public const string SpoolEnabled = nameof(SpoolEnabled);
  public const string StartTime = nameof(StartTime);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TimeOfLastReset = nameof(TimeOfLastReset);
  public const string UntilTime = nameof(UntilTime);
  public const string VerticalResolution = nameof(VerticalResolution);
  public const string WorkOffline = nameof(WorkOffline);
}
