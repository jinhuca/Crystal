using Crystal.Provider.Smbios.Types;

namespace Crystal.Provider.Smbios.Structures;

/// <summary>
/// Discriminated wrapper for a memory error information entry.
/// Provides typed access to the concrete 32-bit or 64-bit implementation
/// while still exposing the common IMemoryErrorInformation interface.
/// </summary>
public sealed class MemoryErrorEntry
{
    public IMemoryErrorInformation Info { get; }

    public T018_MemoryErrorInformation32? As32 => Info as T018_MemoryErrorInformation32;
    public T033_MemoryErrorInformation64? As64 => Info as T033_MemoryErrorInformation64;

    public bool Is32 => As32 is not null;
    public bool Is64 => As64 is not null;

    internal MemoryErrorEntry(IMemoryErrorInformation info)
    {
        Info = info;
    }

    public static MemoryErrorEntry From(IMemoryErrorInformation info) => new MemoryErrorEntry(info);
}
