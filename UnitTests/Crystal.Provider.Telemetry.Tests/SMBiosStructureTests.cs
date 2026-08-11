using System;
using System.Collections.Generic;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

// Exercises the SMBIOS byte-buffer decoders: the InformationBase primitive readers and every
// structure parser (BIOS, System, Enclosure, BaseBoard, Processor, Cache, MemoryDevice). Each
// parser takes a raw structure blob plus the structure's string set; string fields are 1-based
// indices into that set (0 meaning "no string"). Buffers are hand-built so the decoded fields are
// asserted against known offsets — no hardware or firmware access.
public class SMBiosStructureTests {
  private static void W(byte[] b, int offset, ushort value) => BitConverter.GetBytes(value).CopyTo(b, offset);
  private static void D(byte[] b, int offset, uint value) => BitConverter.GetBytes(value).CopyTo(b, offset);
  private static void Q(byte[] b, int offset, ulong value) => BitConverter.GetBytes(value).CopyTo(b, offset);

  // Concrete InformationBase to reach the protected primitive readers directly.
  private sealed class Probe : InformationBase {
    public Probe(byte[] data, IList<string> strings) : base(data, strings) { }
    public byte Byte(int o) => GetByte(o);
    public ushort Word(int o) => GetWord(o);
    public uint Dword(int o) => GetDword(o);
    public ulong Qword(int o) => GetQword(o);
    public string Str(int o) => GetString(o);
  }

  // ---- InformationBase primitive readers ----

  [Fact]
  public void InformationBase_reads_little_endian_scalars() {
    var data = new byte[16];
    data[0] = 0xAB;
    W(data, 1, 0x1234);
    D(data, 3, 0x89ABCDEF);
    Q(data, 7, 0x1122334455667788);
    var probe = new Probe(data, Array.Empty<string>());

    Assert.Equal(0xAB, probe.Byte(0));
    Assert.Equal(0x1234, probe.Word(1));
    Assert.Equal(0x89ABCDEFu, probe.Dword(3));
    Assert.Equal(0x1122334455667788ul, probe.Qword(7));
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(64)]
  public void InformationBase_out_of_range_reads_return_zero(int offset) {
    var probe = new Probe(new byte[8], Array.Empty<string>());

    Assert.Equal(0, probe.Byte(offset));
    Assert.Equal(0, probe.Word(offset));
    Assert.Equal(0u, probe.Dword(offset));
    Assert.Equal(0ul, probe.Qword(offset));
  }

  [Fact]
  public void InformationBase_reads_scalars_as_zero_when_they_straddle_the_buffer_end() {
    // GetWord/Dword/Qword require the whole value to fit; a partial read yields 0, not garbage.
    var probe = new Probe(new byte[4], new List<string>());

    Assert.Equal(0, probe.Word(3));   // needs offsets 3..4, only 0..3 exist
    Assert.Equal(0u, probe.Dword(1)); // needs 1..4
    Assert.Equal(0ul, probe.Qword(0));
  }

  [Fact]
  public void InformationBase_string_index_is_one_based_and_zero_means_no_string() {
    var data = new byte[] { 0x00, 0x01, 0x02, 0x63 };
    var probe = new Probe(data, new List<string> { "first", "second" });

    Assert.Equal(string.Empty, probe.Str(0)); // index byte 0 -> no string
    Assert.Equal("first", probe.Str(1));       // index byte 1 -> strings[0]
    Assert.Equal("second", probe.Str(2));      // index byte 2 -> strings[1]
    Assert.Equal(string.Empty, probe.Str(3));  // index byte 0x63 out of range -> empty
  }

  // ---- BiosInformation ----

  [Fact]
  public void BiosInformation_parses_vendor_version_date_and_legacy_rom_size() {
    var data = new byte[32];
    data[0x04] = 1; // Vendor
    data[0x05] = 2; // Version
    data[0x08] = 3; // Date string
    data[0x09] = 0x0F; // legacy ROM size byte (not 0xFF): (0x0F + 1) * 64 KiB = 1 MiB
    var bios = new BiosInformation(data, new List<string> { "American Megatrends", "F12", "03/15/2021" });

    Assert.Equal("American Megatrends", bios.Vendor);
    Assert.Equal("F12", bios.Version);
    Assert.Equal(new DateTime(2021, 3, 15), bios.Date);
    Assert.Equal(1024ul * 1024ul, bios.Size);
  }

  [Fact]
  public void BiosInformation_uses_extended_rom_size_in_megabytes_when_legacy_byte_is_saturated() {
    var data = new byte[32];
    data[0x09] = 0xFF;   // legacy byte saturated -> defer to extended word
    W(data, 0x18, 16);   // unit bits 00 = MB, value 16 -> 16 MiB
    var bios = new BiosInformation(data, new List<string>());

    Assert.Equal(16ul * 1024ul * 1024ul, bios.Size);
  }

  // ---- SystemInformation ----

  [Fact]
  public void SystemInformation_parses_strings_and_wake_up() {
    var data = new byte[32];
    data[0x04] = 1; // Manufacturer
    data[0x05] = 2; // Product
    data[0x06] = 3; // Version
    data[0x07] = 4; // Serial
    data[0x18] = (byte)SystemWakeUp.PowerSwitch;
    data[0x1A] = 5; // Family
    var system = new SystemInformation(
        data, new List<string> { "Dell Inc.", "XPS 15", "1.2", "SN9", "XPS" });

    Assert.Equal("Dell Inc.", system.ManufacturerName);
    Assert.Equal("XPS 15", system.ProductName);
    Assert.Equal("1.2", system.Version);
    Assert.Equal("SN9", system.SerialNumber);
    Assert.Equal("XPS", system.Family);
    Assert.Equal(SystemWakeUp.PowerSwitch, system.WakeUp);
  }

  // ---- SystemEnclosure ----

  [Fact]
  public void SystemEnclosure_splits_the_type_byte_into_lock_bit_and_type() {
    var data = new byte[24];
    data[0x04] = 1; // Manufacturer
    data[0x05] = 0x80 | (byte)SystemEnclosureType.Desktop; // high bit = lock present
    data[0x06] = 2; // Version
    data[0x07] = 3; // Serial
    data[0x08] = 4; // AssetTag
    data[0x09] = (byte)SystemEnclosureState.Safe;     // BootUpState
    data[0x0A] = (byte)SystemEnclosureState.Safe;     // PowerSupplyState
    data[0x0B] = (byte)SystemEnclosureState.Warning;  // ThermalState
    data[0x0C] = (byte)SystemEnclosureSecurityStatus.None;
    data[0x11] = 5;  // RackHeight
    data[0x12] = 2;  // PowerCords
    data[0x15] = 5;  // SKU string
    var enclosure = new SystemEnclosure(
        data, new List<string> { "Dell", "v1.0", "SER123", "ASSET9", "SKU-X" });

    Assert.True(enclosure.LockDetected);
    Assert.Equal(SystemEnclosureType.Desktop, enclosure.Type);
    Assert.Equal("Dell", enclosure.ManufacturerName);
    Assert.Equal("SER123", enclosure.SerialNumber);
    Assert.Equal("ASSET9", enclosure.AssetTag);
    Assert.Equal(SystemEnclosureState.Safe, enclosure.BootUpState);
    Assert.Equal(SystemEnclosureState.Safe, enclosure.PowerSupplyState);
    Assert.Equal(SystemEnclosureState.Warning, enclosure.ThermalState);
    Assert.Equal(SystemEnclosureSecurityStatus.None, enclosure.SecurityStatus);
    Assert.Equal(5, enclosure.RackHeight);
    Assert.Equal(2, enclosure.PowerCords);
    Assert.Equal("SKU-X", enclosure.SKU);
  }

  [Fact]
  public void SystemEnclosure_reports_no_lock_when_high_bit_is_clear() {
    var data = new byte[24];
    data[0x05] = (byte)SystemEnclosureType.Tower; // no high bit
    var enclosure = new SystemEnclosure(data, new List<string>());

    Assert.False(enclosure.LockDetected);
    Assert.Equal(SystemEnclosureType.Tower, enclosure.Type);
  }

  // ---- BaseBoardInformation ----

  [Fact]
  public void BaseBoardInformation_trims_its_string_fields() {
    var data = new byte[16];
    data[0x04] = 1; // Manufacturer
    data[0x05] = 2; // Product
    data[0x06] = 3; // Version
    data[0x07] = 4; // Serial
    var board = new BaseBoardInformation(
        data, new List<string> { "  ASUS  ", " ROG STRIX ", "Rev 1.0", " MB-SN-1 " });

    Assert.Equal("ASUS", board.ManufacturerName);
    Assert.Equal("ROG STRIX", board.ProductName);
    Assert.Equal("Rev 1.0", board.Version);
    Assert.Equal("MB-SN-1", board.SerialNumber);
  }

  // ---- ProcessorInformation ----

  [Fact]
  public void ProcessorInformation_parses_a_typical_desktop_cpu_record() {
    var data = new byte[64];
    W(data, 0x02, 0x0044);                  // Handle
    data[0x04] = 1;                          // SocketDesignation
    data[0x05] = (byte)ProcessorType.CentralProcessor;
    data[0x06] = (byte)ProcessorFamily.IntelCoreI7;
    data[0x07] = 2;                          // Manufacturer
    Q(data, 0x08, 0x1122334455667788);      // Id
    data[0x10] = 3;                          // Version
    W(data, 0x12, 100);                      // ExternalClock
    W(data, 0x14, 4200);                     // MaxSpeed
    W(data, 0x16, 3600);                     // CurrentSpeed
    data[0x19] = (byte)ProcessorSocket.Other;
    W(data, 0x1A, 0x0050);                   // L1 cache handle
    W(data, 0x1C, 0x0051);                   // L2 cache handle
    W(data, 0x1E, 0x0052);                   // L3 cache handle
    data[0x20] = 4;                          // Serial
    data[0x23] = 8;                          // CoreCount
    data[0x24] = 8;                          // CoreEnabled
    data[0x25] = 16;                         // ThreadCount
    data[0x26] = 0b0001_1100;                // char1: bits 2,3,4 -> 64-bit, MultiCore, HW thread
    data[0x27] = 0b0000_0001;                // char2: bit 0 -> 128-bit
    var cpu = new ProcessorInformation(
        data, new List<string> { "CPU1", "Intel", "  Core i7-9700K  ", "SN12345" });

    Assert.Equal((ushort)0x0044, cpu.Handle);
    Assert.Equal("CPU1", cpu.SocketDesignation);
    Assert.Equal(ProcessorType.CentralProcessor, cpu.ProcessorType);
    Assert.Equal(ProcessorFamily.IntelCoreI7, cpu.Family);
    Assert.Equal("Intel", cpu.ManufacturerName);
    Assert.Equal(0x1122334455667788ul, cpu.Id);
    Assert.Equal("Core i7-9700K", cpu.Version);
    Assert.Equal(100, cpu.ExternalClock);
    Assert.Equal(4200, cpu.MaxSpeed);
    Assert.Equal(3600, cpu.CurrentSpeed);
    Assert.Equal(ProcessorSocket.Other, cpu.Socket);
    Assert.Equal((ushort)0x0050, cpu.L1CacheHandle);
    Assert.Equal((ushort)0x0051, cpu.L2CacheHandle);
    Assert.Equal((ushort)0x0052, cpu.L3CacheHandle);
    Assert.Equal("SN12345", cpu.Serial);
    Assert.Equal(8, cpu.CoreCount);
    Assert.Equal(8, cpu.CoreEnabled);
    Assert.Equal(16, cpu.ThreadCount);
    Assert.Equal(
        ProcessorCharacteristics._64BitCapable | ProcessorCharacteristics.MultiCore |
        ProcessorCharacteristics.HardwareThread | ProcessorCharacteristics._128BitCapable,
        cpu.Characteristics);
  }

  [Fact]
  public void ProcessorInformation_falls_back_to_word_counts_when_byte_counts_are_saturated() {
    var data = new byte[64];
    data[0x23] = 0xFF; W(data, 0x2A, 300); // CoreCount
    data[0x24] = 0xFF; W(data, 0x2C, 288); // CoreEnabled
    data[0x25] = 0xFF; W(data, 0x2E, 576); // ThreadCount
    var cpu = new ProcessorInformation(data, new List<string>());

    Assert.Equal(300, cpu.CoreCount);
    Assert.Equal(288, cpu.CoreEnabled);
    Assert.Equal(576, cpu.ThreadCount);
  }

  [Fact]
  public void ProcessorInformation_reads_the_extended_family_word_when_the_family_byte_escapes() {
    var data = new byte[64];
    data[0x06] = 254;              // escape -> read the extended family word
    W(data, 0x28, (ushort)ProcessorFamily.ArmV7);
    var cpu = new ProcessorInformation(data, new List<string>());

    Assert.Equal(ProcessorFamily.ArmV7, cpu.Family);
  }

  // ---- CacheInformation ----

  [Theory]
  [InlineData("L1 - Cache", CacheDesignation.L1)]
  [InlineData("L2 Cache", CacheDesignation.L2)]
  [InlineData("L3", CacheDesignation.L3)]
  [InlineData("Internal Cache", CacheDesignation.Other)]
  public void CacheInformation_maps_designation_from_the_socket_string(string label, CacheDesignation expected) {
    var data = new byte[24];
    W(data, 0x02, 0x0060);   // Handle
    data[0x04] = 1;          // designation string
    W(data, 0x09, 512);      // Size
    data[0x12] = (byte)CacheAssociativity._8Way;
    var cache = new CacheInformation(data, new List<string> { label });

    Assert.Equal((ushort)0x0060, cache.Handle);
    Assert.Equal(expected, cache.Designation);
    Assert.Equal(512, cache.Size);
    Assert.Equal(CacheAssociativity._8Way, cache.Associativity);
  }

  // ---- MemoryDevice ----

  [Fact]
  public void MemoryDevice_parses_locators_speeds_and_type() {
    var data = new byte[48];
    W(data, 0x0C, 16384);    // Size (not the 0x7FFF escape)
    data[0x10] = 1;          // DeviceLocator
    data[0x11] = 2;          // BankLocator
    data[0x12] = (byte)MemoryType.DDR4;
    W(data, 0x15, 3200);     // Speed
    data[0x17] = 3;          // Manufacturer
    data[0x18] = 4;          // Serial
    data[0x1A] = 5;          // PartNumber
    W(data, 0x20, 3200);     // ConfiguredSpeed
    W(data, 0x26, 1200);     // ConfiguredVoltage
    var dimm = new MemoryDevice(
        data, new List<string> { "DIMM_A1", "BANK 0", " Kingston ", "MSN123", "KF3200" });

    Assert.Equal("DIMM_A1", dimm.DeviceLocator);
    Assert.Equal("BANK 0", dimm.BankLocator);
    Assert.Equal("Kingston", dimm.ManufacturerName);
    Assert.Equal("MSN123", dimm.SerialNumber);
    Assert.Equal("KF3200", dimm.PartNumber);
    Assert.Equal(16384u, dimm.Size);
    Assert.Equal(3200, dimm.Speed);
    Assert.Equal(3200, dimm.ConfiguredSpeed);
    Assert.Equal(1200, dimm.ConfiguredVoltage);
    Assert.Equal(MemoryType.DDR4, dimm.Type);
  }

  [Fact]
  public void MemoryDevice_reads_the_extended_size_dword_when_the_word_size_escapes() {
    var data = new byte[48];
    W(data, 0x0C, 0x7FFF);      // escape -> defer to the extended dword
    D(data, 0x1C, 32768);
    var dimm = new MemoryDevice(data, new List<string>());

    Assert.Equal(32768u, dimm.Size);
  }
}
