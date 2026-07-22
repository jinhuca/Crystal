namespace Crystal.Mmi.HardwareFeatures.SystemEnclosure;

public record SystemEnclosureMetrics(
  string? AssetTag,
  bool? AudibleAlarm,
  string? BreachDescription,
  ushort? CableManagementStrategy,
  string? Caption,
  ushort[]? ChassisTypes,          // Crucial enum array identifying physical form factors
  string? CreationClassName,
  string? Description,
  bool? HeatSinkPresent,
  bool? HotSwappable,
  DateTime? InstallDate,
  string? LastErrorCode,
  bool? LockPresent,
  ushort? SecurityStatus,
  string? SerialNumber,            // Case/Chassis tracking identifier string
  string? SMBIOSAssetTag,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  string? Tag,
  ushort? SecurityBreach,          // 3 = Breach successfully detected
  string? Version,
  bool? VisibleAlarm
) {
  // --- RUNTIME CHASSIS TYPE TRANSLATOR ---

  // Translates the physical ChassisTypes enum value into a clear form factor name
  public string FormFactorName {
    get {
      var firstType = ChassisTypes?.FirstOrDefault();
      return firstType switch {
        1 => "Other / Custom Enclosure",
        2 => "Unknown Form Factor",
        3 => "Desktop",
        4 => "Low Profile Desktop",
        5 => "Pizza Box",
        6 => "Mini Tower",
        7 => "Tower",
        8 => "Portable",
        9 => "Laptop",
        10 => "Notebook",
        11 => "Hand Held / Mobile Device",
        12 => "Docking Station",
        13 => "All in One PC",
        14 => "Sub Notebook",
        15 => "Space-Saving PC",
        16 => "Lunch Box",
        17 => "Main System Chassis",
        18 => "Expansion Chassis",
        19 => "SubChassis",
        20 => "Bus Expansion Chassis",
        21 => "Peripheral Chassis",
        22 => "Storage Enclosure",
        23 => "Rack Mount Chassis (Server)",
        24 => "Main Server Blade",
        _ => "Undocumented Structural Casing"
      };
    }
  }
}
