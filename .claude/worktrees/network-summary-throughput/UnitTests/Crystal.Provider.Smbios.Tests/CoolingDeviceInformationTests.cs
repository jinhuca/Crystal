using System;
using System.Collections.Generic;
using Xunit;
using Crystal.Provider.Smbios;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;

namespace Crystal.Provider.Smbios.Tests;

public class CoolingDeviceInformationTests {
  [Fact]
  public void Decode_ValidCoolingDevice_ParsesFieldsAndSplitsStatusByte() {
    // DSP0134 §7.28: Temperature Probe Handle @0x04, Device Type and Status @0x06,
    // Cooling Unit Group @0x07, OEM DWORD @0x08, Nominal Speed WORD @0x0C,
    // Description @0x0E. Structure length = 0x0F.
    byte[] rawBytes = new byte[0x0F];
    rawBytes[0x00] = 0x1B; // Type 27
    rawBytes[0x01] = 0x0F; // Length 15
    rawBytes[0x04] = 0x05; rawBytes[0x05] = 0x00; // Temperature Probe Handle: 0x0005

    // DeviceTypeAndStatusRaw calculation:
    // Status = OK (0x03) -> 0x03 << 5 = 0x60
    // Type = CabinetFan (0x06) -> 0x06
    // Combined = 0x60 | 0x06 = 0x66
    rawBytes[0x06] = 0x66;

    rawBytes[0x07] = 0x01; // Cooling Unit Group: 1
    rawBytes[0x08] = 0x00; rawBytes[0x09] = 0x00; rawBytes[0x0A] = 0x00; rawBytes[0x0B] = 0x00; // OEM Defined
    rawBytes[0x0C] = 0xB8; rawBytes[0x0D] = 0x0B; // Nominal Speed: 3000 RPM (0x0BB8)
    rawBytes[0x0E] = 1;    // Description string index 1

    var stringTable = new List<string> {
      "Chassis Intake Fan 1"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.CoolingDevice,
      0x0F,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T027_CoolingDevice.Decode(mockStructure);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(0x0005, result.TemperatureProbeHandle);
    Assert.True(result.HasAssociatedProbe);

    Assert.Equal(0x66, result.DeviceTypeAndStatusRaw);
    Assert.Equal(CoolingDeviceType.CabinetFan, result.DeviceType);
    Assert.Equal(CoolingDeviceStatus.OK, result.Status);

    Assert.Equal(1, result.CoolingUnitGroup);
    Assert.Equal((ushort)3000, result.NominalSpeedRpm);
    Assert.True(result.IsSpeedIdentifiable);
    Assert.Equal("Chassis Intake Fan 1", result.Description);
  }

  [Fact]
  public void Decode_UnknownNominalSpeed_SetsFlagsCorrectly() {
    // Arrange: Create a skeleton structure missing a reading tag or providing unknown masks
    byte[] rawBytes = new byte[0x0F];
    rawBytes[0x04] = 0xFF; rawBytes[0x05] = 0xFF; // No Probe (0xFFFF)
    rawBytes[0x06] = 0x43; // Status = Unknown (0x02 << 5 = 0x40), Type = Fan (0x03) -> 0x43
    rawBytes[0x0C] = 0x00; rawBytes[0x0D] = 0x80; // Nominal Speed WORD: 0x8000 (Unknown)

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.CoolingDevice,
      0x0F,
      1,
      rawBytes,
      new List<string>()
    );

    // Act
    var result = T027_CoolingDevice.Decode(mockStructure);

    // Assert
    Assert.False(result.HasAssociatedProbe);
    Assert.Equal(CoolingDeviceType.Fan, result.DeviceType);
    Assert.Equal(CoolingDeviceStatus.Unknown, result.Status);
    Assert.Equal((ushort)0x8000, result.NominalSpeedRpm);
    Assert.False(result.IsSpeedIdentifiable); // The helper evaluated unknown correctly
  }
}
