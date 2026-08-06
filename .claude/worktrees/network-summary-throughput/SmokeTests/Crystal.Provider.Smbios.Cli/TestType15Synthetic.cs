using Crystal.Provider.Smbios.Structures;
using System;
using System.Collections.Generic;

internal static class TestType15Synthetic {
  public static void Run() {
    // Build a minimal SMBIOS structure-table containing:
    // - Type 15 formatted area (length = 11 bytes) with some example fields
    // - string table (double-null)
    // - EndOfTable (type 127, length 4) + empty string table (double-null)
    var bytes = new List<byte>();

    // Type 15 header+formatted area
    // offsets: 0:type,1:length,2-3:handle,4-5:LogAreaLength(WORD),
    // 6-7:LogHeaderStartOffset(WORD), 8:LogHeaderFormat, 9:LogHeaderLength, 10:AccessMethod
    ushort handle15 = 0x1000;
    ushort logAreaLength = 0x0010;     // example
    ushort headerStart = 0x0000;
    byte headerFormat = 1;
    byte headerLen = 4;
    byte accessMethod = 1;

    bytes.Add(15);               // type
    bytes.Add(11);               // length (formatted area size)
    bytes.Add((byte)(handle15 & 0xFF)); // handle lo
    bytes.Add((byte)(handle15 >> 8));   // handle hi
    bytes.Add((byte)(logAreaLength & 0xFF)); // log area length lo
    bytes.Add((byte)(logAreaLength >> 8));   // hi
    bytes.Add((byte)(headerStart & 0xFF)); // header start lo
    bytes.Add((byte)(headerStart >> 8));   // hi
    bytes.Add(headerFormat);     // header format
    bytes.Add(headerLen);        // header length
    bytes.Add(accessMethod);     // access method

    // string table: no strings -> double null
    bytes.Add(0x00);
    bytes.Add(0x00);

    // EndOfTable record
    bytes.Add(127); // type
    bytes.Add(4);   // length
    bytes.Add(0xFF); // handle lo (example)
    bytes.Add(0xFF); // handle hi
                     // string table for EndOfTable (empty)
    bytes.Add(0x00);
    bytes.Add(0x00);

    byte major = 3;
    byte minor = 4;
    var table = SmbiosTable.FromRawTableData(bytes.ToArray(), major, minor);
    Console.WriteLine($"Synthetic table: Raw count = {table.RawStructures.Count}");
    Console.WriteLine($"SystemEventLogs count = {table.SystemEventLogs.Count}");
    foreach (var log in table.SystemEventLogs) {
      Console.WriteLine($"  Handle: 0x{log.Handle:X4}");
      Console.WriteLine($"  LogAreaLength: {log.LogAreaLength}");
      Console.WriteLine($"  LogHeaderStartOffset: {log.LogHeaderStartOffset}");
      Console.WriteLine($"  LogHeaderFormat: {log.LogHeaderFormat}");
      Console.WriteLine($"  LogDataStartOffset: {log.LogDataStartOffset}");
      Console.WriteLine($"  AccessMethod: {log.AccessMethod}");
      Console.WriteLine($"  FormattedArea bytes: {BitConverter.ToString(log.FormattedArea.ToArray())}");
    }
  }
}