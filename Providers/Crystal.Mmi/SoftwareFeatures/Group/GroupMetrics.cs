namespace Crystal.Mmi.SoftwareFeatures.Group;

// Win32_Group is derived from Win32_Account, the same base class as Win32_UserAccount,
// so it shares that class's Domain/LocalAccount/SID/SIDType identity fields.
public record GroupMetrics(
  string? Caption,
  string? Description,
  string? Domain,
  DateTime? InstallDate,
  bool? LocalAccount,
  string? Name,
  string? SID,
  byte? SIDType,
  string? Status
);
