using System.Runtime.InteropServices;

namespace Crystal.Provider.Telemetry.Interop.PowerMonitor;

/// <summary>Vendor identification data reported by the device.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct VendorDataStruct {
  /// <summary>The vendor identifier.</summary>
  public byte VendorId;
  /// <summary>The product identifier.</summary>
  public byte ProductId;
  /// <summary>The firmware version.</summary>
  public byte FwVersion;
}

/// <summary>A single power sensor reading (voltage, current and power).</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PowerSensor {
  /// <summary>The measured voltage.</summary>
  public short Voltage;
  /// <summary>The measured current.</summary>
  public uint Current;
  /// <summary>The measured power.</summary>
  public uint Power;
}

/// <summary>Aggregated sensor readings reported by the device.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct SensorStruct {
  /// <summary>The temperature sensor readings, in 0.1 °C.</summary>
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
  public short[] Ts; // 0.1 °C

  /// <summary>The supply voltage, in mV.</summary>
  public ushort Vdd; // mV
  /// <summary>The fan duty cycle, in percent.</summary>
  public byte FanDuty; // %

  /// <summary>The per-channel power sensor readings.</summary>
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
  public PowerSensor[] PowerReadings;

  /// <summary>The total power across all channels, in mW.</summary>
  public uint TotalPower; // mW
  /// <summary>The total current across all channels, in mA.</summary>
  public uint TotalCurrent; // mA
  /// <summary>The average voltage across all channels, in mV.</summary>
  public ushort AvgVoltage; // mV
  /// <summary>The high-power delivery capability of the device.</summary>
  public HpwrCapability HpwrCapability; // 8-bit enum
  /// <summary>The current fault status bitmask.</summary>
  public ushort FaultStatus;
  /// <summary>The logged fault bitmask.</summary>
  public ushort FaultLog;
}

/// <summary>Fan control configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FanConfigStruct {
  /// <summary>The fan control mode.</summary>
  public FanMode Mode;
  /// <summary>The temperature source used to drive the fan.</summary>
  public TempSource TempSource;
  /// <summary>The minimum fan duty cycle, in percent.</summary>
  public byte DutyMin;
  /// <summary>The maximum fan duty cycle, in percent.</summary>
  public byte DutyMax;
  /// <summary>The minimum temperature of the fan curve.</summary>
  public short TempMin;
  /// <summary>The maximum temperature of the fan curve.</summary>
  public short TempMax;
}

/// <summary>Version 1 of the user interface configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct UiConfigStructV1 {
  /// <summary>The scale used to display current values.</summary>
  public CurrentScale CurrentScale;
  /// <summary>The scale used to display power values.</summary>
  public PowerScale PowerScale;
  /// <summary>The display theme.</summary>
  public Theme Theme;
  /// <summary>The display rotation.</summary>
  public DisplayRotation DisplayRotation;
  /// <summary>The screen timeout mode.</summary>
  public TimeoutMode TimeoutMode;
  /// <summary>Whether screens are cycled automatically.</summary>
  public byte CycleScreens;
  /// <summary>The interval between cycled screens.</summary>
  public byte CycleTime;
  /// <summary>The display timeout duration.</summary>
  public byte Timeout;
}

/// <summary>Version 2 of the user interface configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct UiConfigStructV2 {
  /// <summary>The screen shown by default.</summary>
  public Screen DefaultScreen;
  /// <summary>The scale used to display current values.</summary>
  public CurrentScale CurrentScale;
  /// <summary>The scale used to display power values.</summary>
  public PowerScale PowerScale;
  /// <summary>The display rotation.</summary>
  public DisplayRotation DisplayRotation;
  /// <summary>The screen timeout mode.</summary>
  public TimeoutMode TimeoutMode;
  /// <summary>The bitmask of screens (SCREEN_*) to cycle through.</summary>
  public byte CycleScreens; // bitmask of SCREEN_*
  /// <summary>The interval between cycled screens, in seconds.</summary>
  public byte CycleTime; // seconds
  /// <summary>The display timeout duration, in seconds.</summary>
  public byte Timeout; // seconds
  /// <summary>The primary display color.</summary>
  public uint PrimaryColor;
  /// <summary>The secondary display color.</summary>
  public uint SecondaryColor;
  /// <summary>The highlight display color.</summary>
  public uint HighlightColor;
  /// <summary>The background display color.</summary>
  public uint BackgroundColor;
  /// <summary>The identifier of the background bitmap.</summary>
  public byte BackgroundBitmapId;
  /// <summary>The identifier of the fan bitmap.</summary>
  public byte FanBitmapId;
  /// <summary>The display color inversion setting.</summary>
  public DISPLAY_INVERSION DisplayInversion;
}

/// <summary>Version 1 of the device configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
public struct DeviceConfigStructV1 {
  /// <summary>The CRC checksum of the configuration.</summary>
  public ushort Crc;
  /// <summary>The configuration structure version.</summary>
  public byte Version;

  /// <summary>The user-defined friendly name of the device.</summary>
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
  public byte[] FriendlyName;

  /// <summary>The fan control configuration.</summary>
  public FanConfigStruct FanConfig;
  /// <summary>The display backlight duty cycle, in percent.</summary>
  public byte BacklightDuty;

  /// <summary>Whether faults are shown on the display.</summary>
  public ushort FaultDisplayEnable;
  /// <summary>Whether faults trigger the buzzer.</summary>
  public ushort FaultBuzzerEnable;
  /// <summary>Whether faults trigger a soft power event.</summary>
  public ushort FaultSoftPowerEnable;
  /// <summary>Whether faults trigger a hard power event.</summary>
  public ushort FaultHardPowerEnable;
  /// <summary>The temperature-sensor fault threshold, in 0.1 °C.</summary>
  public short TsFaultThreshold; // 0.1 °C
  /// <summary>The over-current protection fault threshold, in A.</summary>
  public byte OcpFaultThreshold; // A
  /// <summary>The per-wire over-current protection fault threshold, in 0.1 A.</summary>
  public byte WireOcpFaultThreshold; // 0.1A
  /// <summary>The over-power protection fault threshold, in W.</summary>
  public ushort OppFaultThreshold; // W
  /// <summary>The current-imbalance fault threshold, in percent.</summary>
  public byte CurrentImbalanceFaultThreshold; // %
  /// <summary>The minimum load required to evaluate the current-imbalance fault, in A.</summary>
  public byte CurrentImbalanceFaultMinLoad; // A
  /// <summary>The wait time before shutdown, in seconds.</summary>
  public byte ShutdownWaitTime; // seconds
  /// <summary>The logging interval, in seconds.</summary>
  public byte LoggingInterval; // seconds
  /// <summary>The user interface configuration.</summary>
  public UiConfigStructV1 Ui;
}

/// <summary>Version 2 of the device configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
public struct DeviceConfigStructV2 {
  /// <summary>The CRC checksum of the configuration.</summary>
  public ushort Crc;
  /// <summary>The configuration structure version.</summary>
  public byte Version;

  /// <summary>The user-defined friendly name of the device.</summary>
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
  public byte[] FriendlyName;

  /// <summary>The fan control configuration.</summary>
  public FanConfigStruct FanConfig;
  /// <summary>The display backlight duty cycle, in percent.</summary>
  public byte BacklightDuty;

  /// <summary>Whether faults are shown on the display.</summary>
  public ushort FaultDisplayEnable;
  /// <summary>Whether faults trigger the buzzer.</summary>
  public ushort FaultBuzzerEnable;
  /// <summary>Whether faults trigger a soft power event.</summary>
  public ushort FaultSoftPowerEnable;
  /// <summary>Whether faults trigger a hard power event.</summary>
  public ushort FaultHardPowerEnable;
  /// <summary>The temperature-sensor fault threshold, in 0.1 °C.</summary>
  public short TsFaultThreshold; // 0.1 °C
  /// <summary>The over-current protection fault threshold, in A.</summary>
  public byte OcpFaultThreshold; // A
  /// <summary>The per-wire over-current protection fault threshold, in 0.1 A.</summary>
  public byte WireOcpFaultThreshold; // 0.1A
  /// <summary>The over-power protection fault threshold, in W.</summary>
  public ushort OppFaultThreshold; // W
  /// <summary>The current-imbalance fault threshold, in percent.</summary>
  public byte CurrentImbalanceFaultThreshold; // %
  /// <summary>The minimum load required to evaluate the current-imbalance fault, in A.</summary>
  public byte CurrentImbalanceFaultMinLoad; // A
  /// <summary>The wait time before shutdown, in seconds.</summary>
  public byte ShutdownWaitTime; // seconds
  /// <summary>The logging interval, in seconds.</summary>
  public byte LoggingInterval; // seconds
  /// <summary>The averaging configuration for sensor readings.</summary>
  public AVG Average;
  /// <summary>The user interface configuration.</summary>
  public UiConfigStructV1 Ui;
}

/// <summary>Version 3 of the device configuration.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
public struct DeviceConfigStructV3 {
  /// <summary>The CRC checksum of the configuration.</summary>
  public ushort Crc;
  /// <summary>The configuration structure version.</summary>
  public byte Version;

  /// <summary>The user-defined friendly name of the device.</summary>
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
  public byte[] FriendlyName;

  /// <summary>The fan control configuration.</summary>
  public FanConfigStruct FanConfig;
  /// <summary>The display backlight duty cycle, in percent.</summary>
  public byte BacklightDuty;

  /// <summary>Whether faults are shown on the display.</summary>
  public ushort FaultDisplayEnable;
  /// <summary>Whether faults trigger the buzzer.</summary>
  public ushort FaultBuzzerEnable;
  /// <summary>Whether faults trigger a soft power event.</summary>
  public ushort FaultSoftPowerEnable;
  /// <summary>Whether faults trigger a hard power event.</summary>
  public ushort FaultHardPowerEnable;
  /// <summary>The temperature-sensor fault threshold, in 0.1 °C.</summary>
  public short TsFaultThreshold; // 0.1 °C
  /// <summary>The over-current protection fault threshold, in A.</summary>
  public byte OcpFaultThreshold; // A
  /// <summary>The per-wire over-current protection fault threshold, in 0.1 A.</summary>
  public byte WireOcpFaultThreshold; // 0.1A
  /// <summary>The over-power protection fault threshold, in W.</summary>
  public ushort OppFaultThreshold; // W
  /// <summary>The current-imbalance fault threshold, in percent.</summary>
  public byte CurrentImbalanceFaultThreshold; // %
  /// <summary>The minimum load required to evaluate the current-imbalance fault, in A.</summary>
  public byte CurrentImbalanceFaultMinLoad; // A
  /// <summary>The wait time before shutdown, in seconds.</summary>
  public byte ShutdownWaitTime; // seconds
  /// <summary>The logging interval, in seconds.</summary>
  public byte LoggingInterval; // seconds
  /// <summary>The averaging configuration for sensor readings.</summary>
  public AVG Average;
  /// <summary>The user interface configuration.</summary>
  public UiConfigStructV2 Ui;
}
