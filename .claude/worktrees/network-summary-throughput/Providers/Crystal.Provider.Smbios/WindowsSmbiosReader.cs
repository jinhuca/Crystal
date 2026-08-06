using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Crystal.Provider.Smbios;

/// <summary>
/// Reads the raw SMBIOS firmware table from Windows via
/// <c>GetSystemFirmwareTable</c> (kernel32.dll).
///
/// The provider signature is <c>'RSMB'</c> (0x52534D42) and the firmware-table
/// identifier is 0 (the only SMBIOS table).  The returned blob starts with
/// a 8-byte Windows-specific header:
/// <code>
///   Offset  Size  Field
///   0x00     1    Used20CallingMethod (ignore)
///   0x01     1    SMBIOSMajorVersion
///   0x02     1    SMBIOSMinorVersion
///   0x03     1    DmiRevision (ignore)
///   0x04     4    Length   — byte count of the structure table that follows
///   0x08     …   Structure table
/// </code>
/// This layout is documented in the Win32 RawSMBIOSData structure
/// (sysinfoapi.h / MSDN "GetSystemFirmwareTable").
/// </summary>
public static class WindowsSmbiosReader
{
    // 'RSMB' as a little-endian DWORD: 'R'=0x52, 'S'=0x53, 'M'=0x4D, 'B'=0x42
    private const uint ProviderSignature = 0x52534D42u;
    private const uint FirmwareTableId   = 0u;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetSystemFirmwareTable(
        uint firmwareTableProviderSignature,
        uint firmwareTableId,
        IntPtr pFirmwareTableBuffer,
        uint bufferSize);

    /// <summary>
    /// Retrieves the raw SMBIOS structure-table bytes from Windows.
    /// The 8-byte Windows header is stripped; only the pure structure-table
    /// data (ready for <see cref="SmbiosTableParser.Parse"/>) is returned.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when not running on Windows.
    /// </exception>
    /// <exception cref="Win32Exception">
    /// Thrown when <c>GetSystemFirmwareTable</c> returns 0 (see inner message).
    /// </exception>
    public static (byte[] TableData, byte MajorVersion, byte MinorVersion) ReadTableData()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "SMBIOS firmware table access is only supported on Windows.");

        // First call: get the required buffer size.
        uint size = GetSystemFirmwareTable(ProviderSignature, FirmwareTableId, IntPtr.Zero, 0);
        if (size == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "GetSystemFirmwareTable failed to return the required buffer size.");

        var buffer = new byte[size];
        uint written;

        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                written = GetSystemFirmwareTable(
                    ProviderSignature, FirmwareTableId, (IntPtr)ptr, size);
            }
        }

        if (written == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "GetSystemFirmwareTable failed to populate the buffer.");

        // Parse the Windows-specific 8-byte header (RawSMBIOSData).
        //   Offset 0: Used20CallingMethod (unused)
        //   Offset 1: SMBIOSMajorVersion
        //   Offset 2: SMBIOSMinorVersion
        //   Offset 3: DmiRevision (unused)
        //   Offset 4: Length (DWORD, little-endian) — structure table byte count
        //   Offset 8: structure table starts here
        byte majorVersion = buffer[1];
        byte minorVersion = buffer[2];
        uint tableLength  = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));

        const int headerSize = 8;
        int actualLength = (int)Math.Min(tableLength, (uint)(buffer.Length - headerSize));

        var tableData = new byte[actualLength];
        Buffer.BlockCopy(buffer, headerSize, tableData, 0, actualLength);

        return (tableData, majorVersion, minorVersion);
    }
}
