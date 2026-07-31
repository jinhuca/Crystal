using Crystal.Smbios;
using Crystal.Smbios.Types;
using System;
using System.Collections.Generic;
using Xunit;

namespace Crystal.Smbios.Tests;

public class MemoryDeviceTests {
  [Fact]
  public void Decode_ValidDdr4Dimm_ParsesFieldsAndCalculatesSize() {
    // Arrange: Build standard 16GB DDR4 DIMM (16384 Megabytes = 0x4000) running at 3200 MT/s
    byte[] rawBytes = new byte[0x2A];
    rawBytes[0x00] = 0x11; // Type 17
    rawBytes[0x01] = 0x2A; // Length 42
    rawBytes[0x04] = 0x02; rawBytes[0x05] = 0x00; // Array Handle 0x0002
    rawBytes[0x08] = 0x40; rawBytes[0x09] = 0x00; // Total Width: 64 bits
    rawBytes[0x0A] = 0x40; rawBytes[0x0B] = 0x00; // Data Width: 64 bits

    // SizeRaw: 16384 MB = 0x4000
    rawBytes[0x0C] = 0x00; rawBytes[0x0D] = 0x40;
    rawBytes[0x0E] = 0x09; // FormFactor: DIMM
    rawBytes[0x10] = 1;    // Device Locator String index 1
    rawBytes[0x11] = 2;    // Bank Locator String index 2
    rawBytes[0x12] = 0x1A; // MemoryType: DDR4 (0x1A)
    // DSP0134 §7.18: TypeDetail WORD @0x13, Speed WORD @0x15, Manufacturer @0x17,
    // Serial @0x18, AssetTag @0x19, PartNumber @0x1A, Attributes @0x1B,
    // Extended Size DWORD @0x1C, Configured Speed @0x20, voltages @0x22/0x24/0x26,
    // Technology @0x28.
    rawBytes[0x13] = 0x80; rawBytes[0x14] = 0x40; // TypeDetail: Synchronous (0x0080) | Registered (0x4000)

    // Speed: 3200 MT/s (0x0C80)
    rawBytes[0x15] = 0x80; rawBytes[0x16] = 0x0C;
    rawBytes[0x17] = 3;    // Manufacturer index 3
    rawBytes[0x18] = 4;    // Serial Number index 4
    rawBytes[0x1A] = 5;    // Part Number index 5
    rawBytes[0x1B] = 0x02; // Attributes: Rank count 2 (Dual Rank)

    // Configured Speed: 3200 MT/s (0x0C80)
    rawBytes[0x20] = 0x80; rawBytes[0x21] = 0x0C;
    // Configured Voltage: 1200 mV (0x04B0)
    rawBytes[0x26] = 0xB0; rawBytes[0x27] = 0x04;
    rawBytes[0x28] = 0x03; // Technology: DRAM

    var stringTable = new List<string> {
      "DIMM_A2",
      "BANK 0",
      "Crucial Technology",
      "12345678F",
      "CT16G4DFD832A"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.MemoryDevice,
      0x2A,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T017_MemoryDevice.Decode(mockStructure);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsPopulated);
    Assert.Equal(MemoryFormFactor.Dimm, result.FormFactor);
    Assert.Equal(MemoryType.Ddr4, result.Type);
    Assert.Equal("DIMM_A2", result.DeviceLocator);
    Assert.Equal("Crucial Technology", result.Manufacturer);
    Assert.Equal("CT16G4DFD832A", result.PartNumber);

    // 16 GB = 17,179,869,184 Bytes
    Assert.Equal(17179869184L, result.CapacityBytes);
    Assert.Equal(3200, result.SpeedMts);
    Assert.Equal(3200, result.ConfiguredMemorySpeedMts);
    Assert.Equal(1200, result.ConfiguredVoltageMillivolts);
    Assert.Equal(2, result.RankCount);
    Assert.Equal(MemoryTechnology.Dram, result.Technology);
  }

  [Fact]
  public void Decode_HighCapacityDdr5_HandlesExtendedSizeField() {
    // Arrange: Build 64GB DDR5 DIMM triggering Extended Size mapping (SizeRaw = 0x7FFF)
    byte[] rawBytes = new byte[0x2A];
    rawBytes[0x00] = 0x11;
    rawBytes[0x01] = 0x2A;

    // SizeRaw indicating fallback to Extended Size
    rawBytes[0x0C] = 0xFF; rawBytes[0x0D] = 0x7F;
    rawBytes[0x12] = 0x22; // MemoryType: DDR5 (0x22)

    // ExtendedSize DWORD @0x1C: 65536 MB (64GB) -> 0x00010000
    rawBytes[0x1C] = 0x00; rawBytes[0x1D] = 0x00; rawBytes[0x1E] = 0x01; rawBytes[0x1F] = 0x00;

    // Speed WORD @0x15: 5600 MT/s -> 0x15E0
    rawBytes[0x15] = 0xE0; rawBytes[0x16] = 0x15;

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.MemoryDevice,
      0x2A,
      1,
      rawBytes,
      new List<string>()
    );

    // Act
    var result = T017_MemoryDevice.Decode(mockStructure);

    // Assert
    Assert.True(result.IsPopulated);
    Assert.Equal(MemoryType.Ddr5, result.Type);
    Assert.Equal(0x7FFF, result.SizeRaw);
    Assert.Equal(65536u, result.ExtendedSizeMegabytes);

    // 64 GB = 68,719,476,736 Bytes
    Assert.Equal(68719476736L, result.CapacityBytes);
    Assert.Equal(5600, result.SpeedMts);
  }

  [Fact]
  public void Decode_EmptyMemorySlot_EvaluatesPopulatedStateToFalse() {
    // Arrange: Create a non-populated physical mapping slot array layout
    byte[] rawBytes = new byte[0x2A];
    rawBytes[0x0C] = 0x00; rawBytes[0x0D] = 0x00; // SizeRaw = 0 indicates unpopulated slot

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.MemoryDevice,
      0x2A,
      1,
      rawBytes,
      new List<string>()
    );

    // Act
    var result = T017_MemoryDevice.Decode(mockStructure);

    // Assert
    Assert.False(result.IsPopulated);
    Assert.Equal(0L, result.CapacityBytes);
    Assert.Equal(0, result.RankCount);
  }
}
