namespace Crystal.Mmi.Wmi;

/// <summary>
/// Common WMI class names used throughout Crystal.Mmi.
/// Feature-specific metadata should reference these constants instead of hardcoding Win32 class strings.
/// </summary>
public static class WmiClasses
{
    // System Identity
    public const string ComputerSystem = "Win32_ComputerSystem";
    public const string ComputerSystemProduct = "Win32_ComputerSystemProduct";
    public const string OperatingSystem = "Win32_OperatingSystem";

    // Firmware / Board
    public const string Bios = "Win32_BIOS";
    public const string BaseBoard = "Win32_BaseBoard";
    public const string SystemEnclosure = "Win32_SystemEnclosure";
    public const string MotherboardDevice = "Win32_MotherboardDevice";

    // Buses / Device Associations
    public const string Bus = "Win32_Bus";
    public const string DeviceBus = "Win32_DeviceBus";
    public const string DeviceSettings = "Win32_DeviceSettings";
    public const string VideoSettings = "Win32_VideoSettings";
    public const string IDEController = "Win32_IDEController";
    public const string IDEControllerDevice = "Win32_IDEControllerDevice";
    public const string SCSIController = "Win32_SCSIController";
    public const string SCSIControllerDevice = "Win32_SCSIControllerDevice";
    public const string USBControllerDevice = "Win32_USBControllerDevice";
    public const string USBHub = "Win32_USBHub";
    public const string DMAChannel = "Win32_DMAChannel";
    public const string OnBoardDevice = "Win32_OnBoardDevice";
    public const string InfraredDevice = "Win32_InfraredDevice";
    public const string AssociatedProcessorMemory = "Win32_AssociatedProcessorMemory";

    // Processor / Memory
    public const string Processor = "Win32_Processor";
    public const string PhysicalMemory = "Win32_PhysicalMemory";
    public const string PhysicalMemoryArray = "Win32_PhysicalMemoryArray";
    public const string CacheMemory = "Win32_CacheMemory";

    // Storage
    public const string DiskDrive = "Win32_DiskDrive";
    public const string DiskPartition = "Win32_DiskPartition";
    public const string LogicalDisk = "Win32_LogicalDisk";
    public const string Volume = "Win32_Volume";
    public const string PhysicalMedia = "Win32_PhysicalMedia";

    // Display / Audio
    public const string VideoController = "Win32_VideoController";
    public const string DesktopMonitor = "Win32_DesktopMonitor";
    public const string SoundDevice = "Win32_SoundDevice";
    public const string DisplayControllerConfiguration = "Win32_DisplayControllerConfiguration";

    // Network
    public const string NetworkAdapter = "Win32_NetworkAdapter";
    public const string NetworkAdapterConfiguration = "Win32_NetworkAdapterConfiguration";
    public const string NetworkClient = "Win32_NetworkClient";
    public const string NetworkConnection = "Win32_NetworkConnection";
    public const string NetworkLoginProfile = "Win32_NetworkLoginProfile";
    public const string NetworkProtocol = "Win32_NetworkProtocol";

    // USB / Plug and Play
    public const string UsbController = "Win32_USBController";
    public const string USBController = "Win32_USBController";
    public const string PnpEntity = "Win32_PnPEntity";

    // Power
    public const string Battery = "Win32_Battery";

    // Software
    public const string Process = "Win32_Process";
    public const string Thread = "Win32_Thread";
    public const string Service = "Win32_Service";

    // Performance
    public const string PerfRawData = "Win32_PerfRawData";
    public const string PerfFormattedData = "Win32_PerfFormattedData";

    // Future Expansion - Hardware
    public const string SystemSlot = "Win32_SystemSlot";
    public const string Tpm = "Win32_Tpm";
    public const string SerialPort = "Win32_SerialPort";
    public const string ParallelPort = "Win32_ParallelPort";
    public const string Keyboard = "Win32_Keyboard";
    public const string PointingDevice = "Win32_PointingDevice";
    public const string Refrigeration = "Win32_Refrigeration";
    public const string TemperatureProbe = "Win32_TemperatureProbe";
    public const string CurrentProbe = "Win32_CurrentProbe";
    public const string VoltageProbe = "Win32_VoltageProbe";
    public const string PowerManagementEvent = "Win32_PowerManagementEvent";
    public const string CDROMDrive = "Win32_CDROMDrive";
    public const string CdRomDrive = "Win32_CDROMDrive";
    public const string TapeDrive = "Win32_TapeDrive";
    public const string Printer = "Win32_Printer";
    public const string PortableBattery = "Win32_PortableBattery";

    // Future Expansion - User / Startup / Environment
    public const string LogonSession = "Win32_LogonSession";
    public const string UserAccount = "Win32_UserAccount";
    public const string StartupCommand = "Win32_StartupCommand";
    public const string Environment = "Win32_Environment";
    public const string Desktop = "Win32_Desktop";
    public const string UserDesktop = "Win32_UserDesktop";
    public const string TimeZone = "Win32_TimeZone";

    // Drivers / File System / Registry
    public const string SystemDriver = "Win32_SystemDriver";
    public const string Directory = "Win32_Directory";
    public const string Registry = "Win32_Registry";
}
