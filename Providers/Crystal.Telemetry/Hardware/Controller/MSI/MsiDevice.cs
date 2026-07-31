namespace Crystal.Telemetry.Hardware.Controller.MSI;

/// <summary>
/// Identifies a specific MSI CoreLiquid AIO cooler model.
/// </summary>
public enum MsiDeviceType {
  /// <summary>
  /// MSI CoreLiquid S280.
  /// </summary>
  S280,
  /// <summary>
  /// MSI CoreLiquid S360.
  /// </summary>
  S360,
  /// <summary>
  /// MSI CoreLiquid S360 MEG.
  /// </summary>
  S360MEG,
  /// <summary>
  /// MSI CoreLiquid X360.
  /// </summary>
  X360,
  /// <summary>
  /// MSI CoreLiquid X240.
  /// </summary>
  X240,
  /// <summary>
  /// MSI CoreLiquid D360.
  /// </summary>
  D360,
  /// <summary>
  /// MSI CoreLiquid D240.
  /// </summary>
  D240,
}

/// <summary>
/// Describes an MSI CoreLiquid device, including its type and USB vendor/product identifiers.
/// </summary>
public class MsiDevice {
  /// <summary>
  /// Initializes a new instance of the <see cref="MsiDevice"/> class.
  /// </summary>
  /// <param name="msiDeviceType">The MSI device model.</param>
  /// <param name="vendorId">The USB vendor identifier.</param>
  /// <param name="productId">The USB product identifier of the device.</param>
  /// <param name="productIdController">The USB product identifier of the device's controller.</param>
  public MsiDevice(MsiDeviceType msiDeviceType, int vendorId, int productId, int productIdController) {
    DeviceType = msiDeviceType;
    VendorId = vendorId;
    ProductId = productId;
    ProductIdController = productIdController;
  }

  /// <summary>
  /// Gets the MSI device model.
  /// </summary>
  public MsiDeviceType DeviceType { get; }
  /// <summary>
  /// Gets the USB vendor identifier.
  /// </summary>
  public int VendorId { get; }
  /// <summary>
  /// Gets the USB product identifier of the device.
  /// </summary>
  public int ProductId { get; }
  /// <summary>
  /// Gets the USB product identifier of the device's controller.
  /// </summary>
  public int ProductIdController { get; }

  /// <summary>
  /// Gets the human-readable product name for the device.
  /// </summary>
  public string Name {
    get {
      switch (DeviceType) {
        case MsiDeviceType.S280:
          return "MSI CoreLiquid S280";
        case MsiDeviceType.S360:
          return "MSI CoreLiquid S360";
        case MsiDeviceType.S360MEG:
          return "MSI CoreLiquid S360 MEG";
        case MsiDeviceType.X360:
          return "MSI CoreLiquid X360";
        case MsiDeviceType.X240:
          return "MSI CoreLiquid X240";
        case MsiDeviceType.D360:
          return "MSI CoreLiquid D360";
        case MsiDeviceType.D240:
          return "MSI CoreLiquid D240";
        default:
          return "Other";
      }
    }
  }

  //Relevant for further HWMonitoring later
  /// <summary>
  /// Determines whether the given firmware version supports hardware monitor indexes 13 and 14.
  /// </summary>
  /// <param name="firmwareVersion">The device firmware version.</param>
  /// <returns><see langword="true"/> if the firmware supports the additional monitor indexes; otherwise, <see langword="false"/>.</returns>
  public bool SupportsHWMonitorIndex13and14(uint firmwareVersion) {
    switch (DeviceType) {
      case MsiDeviceType.S280:
      case MsiDeviceType.S360:
        return (firmwareVersion & byte.MaxValue) >= 10;
      case MsiDeviceType.S360MEG:
        return (firmwareVersion & byte.MaxValue) >= 7;
      case MsiDeviceType.X360:
      case MsiDeviceType.X240:
        return (firmwareVersion & byte.MaxValue) >= 3;
      case MsiDeviceType.D360:
      case MsiDeviceType.D240:
        return true;
    }

    return false;
  }
}
