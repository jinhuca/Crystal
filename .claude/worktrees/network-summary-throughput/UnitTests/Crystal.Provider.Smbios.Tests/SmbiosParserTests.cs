using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

/// <summary>
/// Unit tests for the SMBIOS parser.  All tests use injected byte arrays —
/// no firmware access required — so they run on any OS and in CI.
///
/// Byte layouts follow DMTF DSP0134 §7 exactly.
/// </summary>
public class SmbiosParserTests {
  // ── Test-helper self-checks ───────────────────────────────────────────────

  [Fact]
  public void MakeStructure_NoStrings_ProducesDoubleNull() {
    var blob = MakeStructure(0x01, 0x0010, new byte[4]);
    Assert.Equal(0x00, blob[^2]);
    Assert.Equal(0x00, blob[^1]);
  }

  [Fact]
  public void MakeStructure_WithStrings_ProducesDoubleNullAfterLastString() {
    var blob = MakeStructure(0x01, 0x0010, new byte[4], new[] { "Hello" });
    Assert.Equal(0x00, blob[^2]);
    Assert.Equal(0x00, blob[^1]);
    Assert.Equal((byte)'H', blob[8]); // 4-byte header + 4-byte payload
  }

  // ── SmbiosTableParser ─────────────────────────────────────────────────────

  [Fact]
  public void Parse_EmptyTable_ReturnsOnlyEoT() {
    var table = MakeTable();
    var structs = SmbiosTableParser.Parse(table);
    Assert.Single(structs);
    Assert.Equal(SmbiosStructureType.EndOfTable, structs[0].Type);
  }

  [Fact]
  public void Parse_SingleStructure_CorrectHeader() {
    var payload = new byte[] { 0x01, 0x02, 0x03 };
    var table = MakeTable(MakeStructure(0x00, 0x0001, payload));
    var structs = SmbiosTableParser.Parse(table);

    Assert.Equal(2, structs.Count); // Type 0 + EoT
    var s = structs[0];
    Assert.Equal(SmbiosStructureType.BiosInformation, s.Type);
    Assert.Equal(0x0001, s.Handle);
    Assert.Equal(4 + payload.Length, s.Length);
  }

  [Fact]
  public void Parse_StringTable_DecodedCorrectly() {
    var payload = new byte[1] { 0x01 };
    var table = MakeTable(MakeStructure(0x00, 0x0002, payload,
        new[] { "ACME Corp", "v1.0", "01/01/2024" }));
    var structs = SmbiosTableParser.Parse(table);

    var s = structs[0];
    Assert.Equal(3, s.Strings.Count);
    Assert.Equal("ACME Corp", s.Strings[0]);
    Assert.Equal("v1.0", s.Strings[1]);
    Assert.Equal("01/01/2024", s.Strings[2]);
  }

  [Fact]
  public void Parse_NoStrings_EmptyStringList() {
    var table = MakeTable(MakeStructure(0x01, 0x0003, new byte[4]));
    var structs = SmbiosTableParser.Parse(table);
    Assert.Empty(structs[0].Strings);
  }

  [Fact]
  public void Parse_MultipleStructures_AllPresent() {
    var t0 = MakeStructure(0, 0x0000, new byte[14], new[] { "Vendor", "1.0", "12/01/2023" });
    var t1 = MakeStructure(1, 0x0001, new byte[23], new[] { "Mfr", "Product", "1.0", "SN001" });
    var table = MakeTable(t0, t1);
    var structs = SmbiosTableParser.Parse(table);

    Assert.Equal(3, structs.Count); // T0 + T1 + EoT
    Assert.Equal(SmbiosStructureType.BiosInformation, structs[0].Type);
    Assert.Equal(SmbiosStructureType.SystemInformation, structs[1].Type);
  }

  [Fact]
  public void Parse_ThreeIndependentStructures_AllParsedCleanly() {
    // Regression test: previously a single trailing 0x00 (instead of 00 00)
    // for the no-strings case caused all structures after the first to
    // misalign and effectively vanish from the parsed result.
    var s1 = MakeStructure(4, 0x0020, new byte[0x26 - 4]);
    var s2 = MakeStructure(4, 0x0021, new byte[0x26 - 4]);
    var s3 = MakeStructure(4, 0x0022, new byte[0x26 - 4]);
    var table = MakeTable(s1, s2, s3);
    var structs = SmbiosTableParser.Parse(table);

    Assert.Equal(4, structs.Count); // 3 structures + EoT
    Assert.Equal(3, structs.Count(x => x.Type == SmbiosStructureType.ProcessorInformation));
  }

  [Fact]
  public void Parse_TruncatedStructure_StopsGracefullyWithoutThrowing() {
    // A structure claiming a length longer than the remaining bytes must
    // be discarded rather than read out of bounds.
    var malformed = new byte[] { 0x01, 0x20, 0x00, 0x00, 0x01, 0x02 }; // length=0x20 but only 6 bytes total
    var structs = SmbiosTableParser.Parse(malformed);
    Assert.Empty(structs);
  }

  [Fact]
  public void Parse_ZeroLengthTable_ReturnsEmpty() {
    var structs = SmbiosTableParser.Parse(Array.Empty<byte>());
    Assert.Empty(structs);
  }

  [Fact]
  public void Parse_LengthShorterThanHeader_Skipped() {
    // Length field of 2 is invalid (header alone is 4 bytes) — must not
    // be parsed as a valid structure or cause an infinite loop.
    var malformed = new byte[] { 0x01, 0x02, 0x00, 0x00, 0x00, 0x00 };
    var structs = SmbiosTableParser.Parse(malformed);
    Assert.Empty(structs);
  }

  // ── Raw field readers ─────────────────────────────────────────────────────

  [Fact]
  public void ReadWord_LittleEndian() {
    var payload = new byte[] { 0x34, 0x12, 0x00, 0x00 };
    var table = MakeTable(MakeStructure(0, 0x0001, payload));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Equal(0x1234, s.ReadWord(0x04));
  }

  [Fact]
  public void ReadDWord_LittleEndian() {
    var payload = new byte[] { 0x78, 0x56, 0x34, 0x12 };
    var table = MakeTable(MakeStructure(0, 0x0001, payload));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Equal(0x12345678u, s.ReadDWord(0x04));
  }

  [Fact]
  public void ReadQWord_LittleEndian() {
    var payload = new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
    var table = MakeTable(MakeStructure(0, 0x0001, payload));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Equal(0x0102030405060708UL, s.ReadQWord(0x04));
  }

  [Fact]
  public void ReadGuid_DecodesRfc4122ByteOrder() {
    var payload = new byte[]
    {
            0x33, 0x22, 0x11, 0x00,
            0x55, 0x44,
            0x77, 0x66,
            0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
    };
    var table = MakeTable(MakeStructure(0, 0x0001, payload));
    var s = SmbiosTableParser.Parse(table)[0];
    var guid = s.ReadGuid(0x04);

    Assert.Equal(new Guid("00112233-4455-6677-8899-AABBCCDDEEFF"), guid);
  }

  [Fact]
  public void GetString_IndexZero_ReturnsNull() {
    var table = MakeTable(MakeStructure(0, 0x0001, new byte[4], new[] { "OnlyString" }));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Null(s.GetString(0));
  }

  [Fact]
  public void GetString_IndexBeyondCount_ReturnsNull() {
    var table = MakeTable(MakeStructure(0, 0x0001, new byte[4], new[] { "OnlyString" }));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Null(s.GetString(5));
  }

  [Fact]
  public void GetString_ValidIndex_ReturnsString() {
    var table = MakeTable(MakeStructure(0, 0x0001, new byte[4], new[] { "First", "Second" }));
    var s = SmbiosTableParser.Parse(table)[0];
    Assert.Equal("First", s.GetString(1));
    Assert.Equal("Second", s.GetString(2));
  }

  // ── BiosInformation decoder ───────────────────────────────────────────────

  private static byte[] MakeBiosPayload(
      byte vendor = 1,
      byte version = 2,
      byte releaseDate = 3,
      ulong characteristics = 0,
      byte biosMajor = 1,
      byte biosMinor = 2,
      byte ecMajor = 3,
      byte ecMinor = 4) {
    var payload = new byte[0x14];
    payload[0x00] = vendor;
    payload[0x01] = version;
    payload[0x04] = releaseDate;
    var charBytes = BitConverter.GetBytes(characteristics);
    charBytes.CopyTo(payload, 0x06);
    payload[0x10] = biosMajor;
    payload[0x11] = biosMinor;
    payload[0x12] = ecMajor;
    payload[0x13] = ecMinor;
    return payload;
  }

  [Fact]
  public void BiosInformation_Strings_DecodedCorrectly() {
    var payload = MakeBiosPayload(vendor: 1, version: 2, releaseDate: 3);
    var table = MakeTable(MakeStructure(0, 0x0000, payload,
        new[] { "AMI", "1.2.3", "11/15/2023" }));
    var smbios = SmbiosTable.FromRawTableData(table);

    var bios = smbios.Bios;
    Assert.NotNull(bios);
    Assert.Equal("AMI", bios!.Vendor);
    Assert.Equal("1.2.3", bios.Version);
    Assert.Equal("11/15/2023", bios.ReleaseDate);
  }

  [Fact]
  public void BiosInformation_ReleaseVersion_DecodedCorrectly() {
    var payload = MakeBiosPayload(biosMajor: 10, biosMinor: 22, ecMajor: 5, ecMinor: 0);
    var table = MakeTable(MakeStructure(0, 0x0000, payload,
        new[] { "Vendor", "Ver", "Date" }));
    var smbios = SmbiosTable.FromRawTableData(table);

    var bios = smbios.Bios;
    Assert.NotNull(bios);
    Assert.Equal(10, bios!.BiosMajorRelease);
    Assert.Equal(22, bios.BiosMinorRelease);
    Assert.Equal(5, bios.EcFirmwareMajor);
    Assert.Equal(0, bios.EcFirmwareMinor);
  }

  [Fact]
  public void BiosInformation_RomSizeBytes_SingleKiB() {
    var payload = MakeBiosPayload();
    payload[0x05] = 0x07; // RomSize: (7+1) × 64 KiB = 512 KiB
    var table = MakeTable(MakeStructure(0, 0x0000, payload, new[] { "V", "1", "D" }));
    var smbios = SmbiosTable.FromRawTableData(table);
    Assert.Equal(512L * 1024, smbios.Bios!.RomSizeBytes);
  }

  // ── SystemInformation decoder ─────────────────────────────────────────────

  [Fact]
  public void SystemInformation_UUID_DecodedCorrectly() {
    var payload = new byte[0x19];
    payload[0x00] = 1;
    payload[0x01] = 2;
    payload[0x02] = 3;
    payload[0x03] = 4;
    var uuidBytes = new byte[]
    {
            0x33, 0x22, 0x11, 0x00,
            0x55, 0x44,
            0x77, 0x66,
            0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
    };
    uuidBytes.CopyTo(payload, 0x04);
    payload[0x14] = 0x06;

    var table = MakeTable(MakeStructure(1, 0x0001, payload,
        new[] { "ASUS", "ROG Strix", "1.0", "SN12345" }));
    var smbios = SmbiosTable.FromRawTableData(table);

    var sys = smbios.System;
    Assert.NotNull(sys);
    Assert.Equal("ASUS", sys!.Manufacturer);
    Assert.Equal("ROG Strix", sys.ProductName);
    Assert.Equal("SN12345", sys.SerialNumber);
    Assert.Equal(new Guid("00112233-4455-6677-8899-AABBCCDDEEFF"), sys.Uuid);
    Assert.Equal(SystemWakeUpType.PowerSwitch, sys.WakeUpType);
  }

  // ── ProcessorInformation decoder ──────────────────────────────────────────

  [Fact]
  public void ProcessorInformation_CoreAndThreadCounts_Correct() {
    var payload = new byte[0x30 - 4];
    payload[0x00] = 1;
    payload[0x01] = (byte)ProcessorType.CentralProcessor;
    payload[0x02] = 0xB3;
    payload[0x03] = 2;
    payload[0x0C] = 3;
    payload[0x0D] = 0x04;
    payload[0x10] = 0x50; payload[0x11] = 0x14; // MaxSpeed = 0x1450 = 5200 MHz
    payload[0x12] = 0x50; payload[0x13] = 0x14; // CurrentSpeed
    payload[0x14] = 0x41; // Status: bit6 set = populated
    payload[0x15] = (byte)ProcessorUpgrade.SocketLGA1700;
    payload[0x1F] = 24; // CoreCount
    payload[0x20] = 24; // CoreEnabled
    payload[0x21] = 32; // ThreadCount

    var table = MakeTable(MakeStructure(4, 0x0004, payload,
        new[] { "CPU0", "Intel(R) Corporation", "Intel(R) Core(TM) i9-13900K" }));
    var smbios = SmbiosTable.FromRawTableData(table);

    var cpu = smbios.ProcessorInformation.FirstOrDefault();
    Assert.NotNull(cpu);
    Assert.True(cpu!.IsPopulated);
    Assert.Equal(5200, cpu.MaxSpeedMhz);
    Assert.Equal((byte)ProcessorUpgrade.SocketLGA1700, (byte)cpu.ProcessorUpgrade);
    Assert.Equal(24, cpu.LogicalCoreCount);
    Assert.Equal(32, cpu.LogicalThreadCount);
    Assert.Equal("CPU0", cpu.SocketDesignation);
  }

  // ── MemoryDevice decoder ──────────────────────────────────────────────────

  [Fact]
  public void MemoryDevice_SizeMiB_CalculatedCorrectly() {
    // Pre-allocate pure data footprint payload (42 bytes spec size minus 4 header bytes = 38 bytes)
    var payload = new byte[0x2A - 4];

    // Size field is at absolute offset 0x0C -> relative offset is 0x0C - 4 = 0x08
    // 0x2000 = 8192 MB
    payload[0x08] = 0x00;
    payload[0x09] = 0x20;

    // Form Factor is at absolute 0x0E -> relative offset is 0x0E - 4 = 0x0A
    payload[0x0A] = (byte)MemoryFormFactor.Dimm;

    // Memory Type is at absolute 0x12 -> relative offset is 0x12 - 4 = 0x0E
    payload[0x0E] = (byte)MemoryType.Ddr5;

    // Pass it to the test helper table builder engine
    var table = MakeTable(MakeStructure(17, 0x0011, payload, new[] { "DIMM0", "BANK 0" }));
    var smbios = SmbiosTable.FromRawTableData(table);

    var mem = smbios.MemoryDevices.FirstOrDefault();

    // Assert
    Assert.NotNull(mem);
    Assert.True(mem!.IsPopulated);

    // Convert capacity bytes back to MiB for validation
    long calculatedMiB = mem.CapacityBytes / (1024L * 1024L);
    Assert.Equal(8192L, calculatedMiB);
  }


  [Fact]
  public void MemoryDevice_NotInstalled_SizeMibIsZero() {
    // Allocate a standard length payload buffer
    var payload = new byte[0x2A];

    // Size field at 0x0C left at 0x0000 indicates an unpopulated slot
    payload[0x0C] = 0x00;
    payload[0x0D] = 0x00;

    var table = MakeTable(MakeStructure(17, 0x0012, payload));
    var smbios = SmbiosTable.FromRawTableData(table);

    var mem = smbios.MemoryDevices.First();

    // Assert using our updated structural design
    Assert.False(mem.IsPopulated);
    Assert.Equal(0L, mem.CapacityBytes);
  }

  [Fact]
  public void MemoryDevice_UnknownSize_SizeMibIsNull() {
    var payload = new byte[0x2A];

    // SMBIOS Spec: 0xFFFF at offset 0x0C indicates the slot size is entirely Unknown
    payload[0x0C] = 0xFF;
    payload[0x0D] = 0xFF;

    var table = MakeTable(MakeStructure(17, 0x0013, payload));
    var smbios = SmbiosTable.FromRawTableData(table);

    var mem = smbios.MemoryDevices.First();

    // Assert that our validation engine successfully marks an unknown slot as unpopulated
    Assert.False(mem.IsPopulated);
    Assert.Equal(0L, mem.CapacityBytes);
  }

  [Fact]
  public void MemoryDevice_KibGranularity_ConvertedToMibCorrectly() {
    // Pre-allocate pure data footprint payload (42 bytes spec size minus 4 header bytes = 38 bytes)
    var payload = new byte[0x2A - 4];

    // Bit 15 set indicates value is in KiB. 
    // 0x8000 | 2048 (0x0800) = 0x8800 -> 2048 KiB = 2 MiB.
    ushort rawValue = 0x8800;

    // SMBIOS Spec: Write the little-endian bytes to the relative Size offset (0x0C - 4 = 0x08)
    payload[0x08] = (byte)rawValue;
    payload[0x09] = (byte)(rawValue >> 8);

    var table = MakeTable(MakeStructure(17, 0x0014, payload));
    var smbios = SmbiosTable.FromRawTableData(table);
    var mem = smbios.MemoryDevices.First();

    // Assert using our updated structural design
    Assert.NotNull(mem);
    Assert.True(mem.IsPopulated);

    // 2048 KiB = 2,097,152 Bytes. In MiB: 2,097,152 / (1024 * 1024) = 2
    long calculatedMiB = mem.CapacityBytes / (1024L * 1024L);
    Assert.Equal(2L, calculatedMiB);
  }


  // ── SmbiosTable aggregate helpers ─────────────────────────────────────────

  [Fact]
  public void TotalInstalledMemoryMiB_SumsPopulatedSlots() {
    static byte[] MakeMemSlot(ushort handle, ushort sizeMiB) {
      var payload = new byte[0x28 - 4];
      payload[0x08] = (byte)sizeMiB;
      payload[0x09] = (byte)(sizeMiB >> 8);
      return MakeStructure(17, handle, payload);
    }

    var table = MakeTable(MakeMemSlot(0x10, 8192), MakeMemSlot(0x11, 8192), MakeMemSlot(0x12, 0));
    var smbios = SmbiosTable.FromRawTableData(table);

    Assert.Equal(2, smbios.InstalledMemoryDevices.Count());
    Assert.Equal(16384L, smbios.TotalInstalledMemoryMiB);
  }

  [Fact]
  public void PopulatedProcessors_FiltersUnpopulatedSockets() {
    static byte[] MakeCpu(ushort handle, bool populated) {
      var payload = new byte[0x26 - 4];
      payload[0x14] = populated ? (byte)0x41 : (byte)0x01;
      return MakeStructure(4, handle, payload);
    }

    var table = MakeTable(MakeCpu(0x20, true), MakeCpu(0x21, false), MakeCpu(0x22, true));
    var smbios = SmbiosTable.FromRawTableData(table);

    Assert.Equal(3, smbios.ProcessorInformation.Count);
    Assert.Equal(2, smbios.PopulatedProcessors.Count());
  }

  [Fact]
  public void Bios_System_Baseboard_ConvenienceProperties_ReturnFirstOrNull() {
    var bios = MakeStructure(0, 0x0000, MakeBiosPayload(), new[] { "V", "1", "D" });
    var table = MakeTable(bios);
    var smbios = SmbiosTable.FromRawTableData(table);

    Assert.NotNull(smbios.Bios);
    Assert.Null(smbios.System);       // no Type 1 present
    Assert.Null(smbios.Baseboard);    // no Type 2 present
  }

  [Fact]
  public void FromRawTableData_MalformedStructureSkipped_RestStillParsed() {
    // First BIOS structure has a payload too short for full decode at
    // some offsets beyond its declared Length — decoder must not throw
    // and must not prevent later structures from being decoded.
    var shortBios = MakeStructure(0, 0x0000, new byte[4]); // minimal payload, no crash expected
    var validCpu = MakeStructure(4, 0x0001, new byte[0x1A - 4]);
    var table = MakeTable(shortBios, validCpu);
    var smbios = SmbiosTable.FromRawTableData(table);

    Assert.Single(smbios.ProcessorInformation);
  }

  [Fact]
  public void FromRawTableData_PassesThroughVersionNumbers() {
    var table = MakeTable();
    var smbios = SmbiosTable.FromRawTableData(table, majorVersion: 3, minorVersion: 6);
    Assert.Equal(3, smbios.MajorVersion);
    Assert.Equal(6, smbios.MinorVersion);
  }
}
