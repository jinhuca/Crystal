namespace Crystal.Mmi.SoftwareFeatures.COMClass;

/// <summary>
/// Metrics record for WMI class <c>Win32_COMClass</c>.
/// Abstract base class (CIM_LogicalElement) for every registered COM component.
/// Querying it directly returns instances from every concrete subclass registered on the
/// system (e.g. Win32_ClassicCOMClass, Win32_ComClassEmulator, Win32_ComClassAutoEmulator) —
/// the same "query the abstract base, get the shared fields" pattern already used for
/// Win32_Perf / Win32_PerfRawData / Win32_PerfFormattedData. Subclass-specific identity
/// fields (CLSID, ProgId, InprocServer32, etc., found on Win32_ClassicCOMClass) are not
/// surfaced here.
/// </summary>
public record COMClassMetrics(
    string? Caption,
    string? Description,
    DateTime? InstallDate,
    string? Name,
    string? Status
);
