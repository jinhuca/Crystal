using System;
using System.Collections.Generic;
using Xunit;
using Crystal.Provider.Smbios.Types;
using Crystal.Provider.Smbios.Structures;

namespace Crystal.Provider.Smbios.Tests;

public class ChassisInformationTests {
  [Fact]
  public void Decode_ValidStructureWithChassisLocked_ParsesCorrectly() {
    // Arrange: Build up raw byte payload for a locked Laptop structure
    byte[] rawBytes = new byte[0x18];
    rawBytes[0x00] = 0x03; // Type 3
    rawBytes[0x01] = 0x18; // Length 24
    rawBytes[0x04] = 1;    // Manufacturer String index 1
    rawBytes[0x05] = 0x89; // TypeRaw: Bit 7 set (Locked), Bits 6:0 = 0x09 (Laptop)
    rawBytes[0x06] = 2;    // Version String index 2
    rawBytes[0x07] = 3;    // Serial Number String index 3
    rawBytes[0x08] = 4;    // Asset Tag String index 4
    rawBytes[0x09] = 0x03; // BootUpState: Safe
    rawBytes[0x0A] = 0x03; // PowerSupplyState: Safe
    rawBytes[0x0B] = 0x04; // ThermalState: Warning
    rawBytes[0x0C] = 0x03; // SecurityStatus: None
    rawBytes[0x0D] = 0xAA; // OEM Defined DWord byte 1
    rawBytes[0x0E] = 0xBB; // OEM Defined DWord byte 2
    rawBytes[0x0F] = 0xCC; // OEM Defined DWord byte 3
    rawBytes[0x10] = 0xDD; // OEM Defined DWord byte 4
    rawBytes[0x11] = 0x02; // Height: 2 U
    rawBytes[0x12] = 0x01; // NumberOfPowerCords: 1
    rawBytes[0x15] = 0x00; // ContainedElementCount: 0
    rawBytes[0x16] = 0x00; // ContainedElementRecordLength: 0

    var stringTable = new List<string> {
      "Custom OEM Corp",
      "Rev 1.0",
      "SN-987654321",
      "TAG-00123"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.ChassisInformation,
      0x18,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T003_ChassisInformation.Decode(mockStructure);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Custom OEM Corp", result.Manufacturer);
    Assert.Equal(0x89, result.TypeRaw);
    Assert.Equal(PhysicalChassisType.Laptop, result.ChassisType);
    Assert.Equal(ChassisLockState.Present, result.LockState);
    Assert.True(result.IsChassisLocked);

    Assert.Equal("Rev 1.0", result.Version);
    Assert.Equal("SN-987654321", result.SerialNumber);
    Assert.Equal("TAG-00123", result.AssetTag);

    Assert.Equal(ChassisState.Safe, result.BootUpState);
    Assert.Equal(ChassisState.Safe, result.PowerSupplyState);
    Assert.Equal(ChassisState.Warning, result.ThermalState);
    Assert.Equal(ChassisSecurityStatus.None, result.SecurityStatus);

    Assert.Equal(0xDDCCBBAAu, result.OEMDefined);
    Assert.Equal(2, result.Height);
    Assert.True(result.HasHeightInfo);
    Assert.Equal(1, result.NumberOfPowerCords);
    Assert.True(result.HasPowerCordInfo);
    Assert.Null(result.SkuNumber);
  }

  [Fact]
  public void Decode_VariableContainedElements_ShiftsSkuOffsetCorrectly() {
    // DSP0134 §7.4: Contained Element Count @0x13, Record Length @0x14,
    // Contained Elements @0x15, SKU Number string @0x15+n*m.
    // 2 elements spanning 3 bytes each -> SKU at 0x15 + 6 = 0x1B.
    int expectedSkuOffset = 0x15 + (2 * 3); // 0x1B
    byte totalLength = (byte)(expectedSkuOffset + 1); // 0x1C

    byte[] rawBytes = new byte[totalLength];
    rawBytes[0x00] = 0x03;
    rawBytes[0x01] = totalLength;
    rawBytes[0x05] = 0x03; // TypeRaw: Desktop (Unlocked)
    rawBytes[0x13] = 0x02; // Count
    rawBytes[0x14] = 0x03; // Length per record

    // Seeding element memory arrays
    rawBytes[0x15] = 0x11; rawBytes[0x16] = 0x22; rawBytes[0x17] = 0x33;
    rawBytes[0x18] = 0x44; rawBytes[0x19] = 0x55; rawBytes[0x1A] = 0x66;

    // SKU location placement pointer
    rawBytes[expectedSkuOffset] = 1;

    var stringTable = new List<string> {
      "SKU-MODEL-XYZ"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.ChassisInformation,
      totalLength,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T003_ChassisInformation.Decode(mockStructure);

    // Assert
    Assert.False(result.IsChassisLocked);
    Assert.Equal(PhysicalChassisType.Desktop, result.ChassisType);
    Assert.Equal(2, result.ContainedElementCount);
    Assert.Equal(3, result.ContainedElementRecordLength);
    Assert.Equal(6, result.ContainedElements.Length);
    Assert.Equal(0x33, result.ContainedElements[2]);
    Assert.Equal("SKU-MODEL-XYZ", result.SkuNumber);
  }
}

// Clean compilation constructor match passing down straight to baseline SmbiosRawStructure logic
internal class MockSmbiosRawStructure : SmbiosRawStructure {
  public MockSmbiosRawStructure(
    SmbiosStructureType type,
    byte length,
    ushort handle,
    ReadOnlyMemory<byte> formattedArea,
    IReadOnlyList<string> strings)
    : base(type, length, handle, formattedArea, strings) {
  }
}
