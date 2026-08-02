namespace Crystal.Provider.Mmi.HardwareFeatures.SMBIOSMemory;

public record SMBIOSMemoryMetrics(
    ushort? Access,
    string? AdditionalErrorData,
    ushort? Availability,
    ulong? BlockSize,
    string? Caption,
    uint? ConfigManagerErrorCode,
    bool? ConfigManagerUserConfig,
    bool? CorrectableError,
    string? CreationClassName,
    string? Description,
    ulong? EndingAddress,
    ushort? ErrorAccess,
    ulong? ErrorAddress,
    bool? ErrorCleared,
    ushort? ErrorCorrectType,
    string? ErrorData,
    ushort? ErrorDataOrder,
    string? ErrorDescription,
    ushort? ErrorInfo,
    string? ErrorMethodology,
    ulong? ErrorResolution,
    DateTime? ErrorTime,
    uint? ErrorTransferSize,
    DateTime? InstallDate,
    uint? LastErrorCode,
    string? Name,
    ulong? NumberOfBlocks,
    string? OtherErrorDescription,
    string? PNPDeviceID,
    ushort[]? PowerManagementCapabilities,
    bool? PowerManagementSupported,
    string? Purpose,
    ulong? StartingAddress,
    string? Status,
    ushort? StatusInfo,
    string? SystemCreationClassName,
    bool? SystemLevelAddress,
    string? SystemName)
{
    public ulong? CapacityBytes => BlockSize.HasValue && NumberOfBlocks.HasValue
        ? BlockSize.Value * NumberOfBlocks.Value
        : null;

    public double? CapacityInGB => CapacityBytes.HasValue
        ? Math.Round(CapacityBytes.Value / 1024d / 1024d / 1024d, 2)
        : null;
}
