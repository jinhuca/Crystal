namespace Crystal.Provider.Mmi.SoftwareFeatures.QuickFixEngineering;

// Win32_QuickFixEngineering only returns updates applied via Component Based Servicing
// (CBS) — hotfixes installed through Windows Update / MSI are not reflected here.
public record QuickFixEngineeringMetrics(
  string? Caption,
  string? CSName,          // Name of the computer system the fix applies to
  string? Description,
  string? FixComments,
  string? HotFixID,         // e.g. "KB4533002"
  DateTime? InstallDate,
  string? InstalledBy,
  string? InstalledOn,       // Free-form string; format varies by OS, not always parseable as a date
  string? Name,
  string? ServicePackInEffect,
  string? Status
);
