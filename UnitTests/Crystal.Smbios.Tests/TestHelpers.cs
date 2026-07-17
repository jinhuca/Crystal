using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystal.Smbios.Tests;

/// <summary>
/// Shared byte-assembly helpers for constructing synthetic SMBIOS structure
/// tables in tests, without needing firmware access.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Builds a raw structure table entry:
    ///   [header (type, length, handle)]  [payload]  [strings]  [double-null]
    /// </summary>
    public static byte[] MakeStructure(
        byte type,
        ushort handle,
        byte[] payload,          // bytes at offset 4+ (after header)
        IEnumerable<string>? strings = null)
    {
        var stringBytes = new List<byte>();
        var stringList  = (strings ?? Array.Empty<string>()).ToList();

        if (stringList.Count == 0)
        {
            // No strings present: string table is an immediate double-null (00 00).
            stringBytes.Add(0x00);
            stringBytes.Add(0x00);
        }
        else
        {
            // Each string is individually null-terminated; the table as a
            // whole ends with one extra null after the last string, giving
            // a double-null at the very end.
            foreach (var s in stringList)
            {
                stringBytes.AddRange(Encoding.Latin1.GetBytes(s));
                stringBytes.Add(0x00);
            }
            stringBytes.Add(0x00);
        }

        byte length = (byte)(4 + payload.Length);
        var blob    = new byte[4 + payload.Length + stringBytes.Count];

        blob[0] = type;
        blob[1] = length;
        blob[2] = (byte)handle;
        blob[3] = (byte)(handle >> 8);

        payload.CopyTo(blob, 4);
        stringBytes.ToArray().CopyTo(blob, 4 + payload.Length);

        return blob;
    }

    /// <summary>
    /// Concatenates several structure blobs into one table byte[],
    /// automatically appending the mandatory End-of-Table (Type 127) marker.
    /// </summary>
    public static byte[] MakeTable(params byte[][] structures)
    {
        var eot = MakeStructure(127, 0x7E00, Array.Empty<byte>());
        return structures.Append(eot).SelectMany(b => b).ToArray();
    }
}
