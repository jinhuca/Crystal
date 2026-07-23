namespace Crystal.Mmi.SoftwareFeatures.UserAccount;

public record UserAccountMetrics(
  uint? AccountType,
  string? Caption,
  string? Description,
  bool? Disabled,
  string? Domain,
  string? FullName,
  DateTime? InstallDate,
  bool? LocalAccount,
  bool? Lockout,
  string? Name,
  bool? PasswordChangeable,
  bool? PasswordExpires,
  bool? PasswordRequired,
  string? SID,
  byte? SIDType,
  string? Status
);
