using System;
using System.Collections.Generic;
using Xunit;
using Crystal.Smbios;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;

namespace Crystal.Smbios.Tests;

public class TemperatureProbeInformationTests {
  [Fact]
  public void Decode_ValidTemperatureProbe_ParsesFieldsAndSplitsStatusByte() {
    // Arrange: Create a comprehensive Type 28 structure matching SMBIOS layout specs
    // DSP0134 §7.29: LocationAndStatus @0x05, Maximum @0x06, Minimum @0x08,
    // Resolution @0x0A, Tolerance @0x0C, Accuracy @0x0E, OEM DWORD @0x10,
    // Nominal @0x14. Structure length = 0x16.
    byte[] rawBytes = new byte[0x16];
    rawBytes[0x00] = 0x1C; // Type 28
    rawBytes[0x01] = 0x16; // Length 22
    rawBytes[0x04] = 1;    // Description string index 1

    // LocationAndStatusRaw calculation:
    // Status = OK (0x03) -> 0x03 << 5 = 0x60
    // Location = Processor (0x03) -> 0x03
    // Combined = 0x60 | 0x03 = 0x63
    rawBytes[0x05] = 0x63;

    // Max Value: 85.0°C -> 850 in tenths -> 0x0352
    rawBytes[0x06] = 0x52; rawBytes[0x07] = 0x03;
    // Min Value: -10.0°C -> -100 in tenths -> Two's complement 0xFF9C
    rawBytes[0x08] = 0x9C; rawBytes[0x09] = 0xFF;
    // Resolution: 0.5°C -> 5 in tenths -> 0x0005
    rawBytes[0x0A] = 0x05; rawBytes[0x0B] = 0x00;
    // Tolerance: 2.0°C -> 20 in tenths -> 0x0014
    rawBytes[0x0C] = 0x14; rawBytes[0x0D] = 0x00;
    // Accuracy: 99% representation -> 0x0063
    rawBytes[0x0E] = 0x63; rawBytes[0x0F] = 0x00;
    // OEM Defined
    rawBytes[0x10] = 0x00; rawBytes[0x11] = 0x00; rawBytes[0x12] = 0x00; rawBytes[0x13] = 0x00;
    // Nominal Value: 35.0°C -> 350 in tenths -> 0x015E
    rawBytes[0x14] = 0x5E; rawBytes[0x15] = 0x01;

    var stringTable = new List<string> {
      "CPU Core Thermal Diode"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.TemperatureProbe,
      0x16,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T028_TemperatureProbeInformation.Decode(mockStructure);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("CPU Core Thermal Diode", result.Description);

    Assert.Equal(0x63, result.LocationAndStatusRaw);
    Assert.Equal(TemperatureProbeLocation.Processor, result.Location);
    Assert.Equal(TemperatureProbeStatus.OK, result.Status);

    Assert.Equal(850, result.MaximumValueRaw);
    Assert.Equal(85.0, result.MaximumValueCelsius);

    Assert.Equal(-100, result.MinimumValueRaw);
    Assert.Equal(-10.0, result.MinimumValueCelsius);

    Assert.Equal(5, result.ResolutionRaw);
    Assert.Equal(0.5, result.ResolutionCelsius);

    Assert.Equal(20, result.ToleranceRaw);
    Assert.Equal(2.0, result.ToleranceCelsius);

    Assert.Equal(99u, result.Accuracy);
    Assert.Equal(350, result.NominalValueRaw);
    Assert.Equal(35.0, result.NominalValueCelsius);
  }

  [Fact]
  public void Decode_UnknownThermalThresholds_ReturnsNullForCelsiusConversions() {
    // Arrange: Create a minimal Type 28 layout tracking unknown threshold markers (0x8000)
    byte[] rawBytes = new byte[0x16];
    rawBytes[0x05] = 0x42; // Status = Unknown (0x40), Location = Unknown (0x02) -> 0x42
    rawBytes[0x06] = 0x00; rawBytes[0x07] = 0x80; // Max Value: 0x8000
    rawBytes[0x08] = 0x00; rawBytes[0x09] = 0x80; // Min Value: 0x8000
    rawBytes[0x0A] = 0x00; rawBytes[0x0B] = 0x80; // Resolution: 0x8000
    rawBytes[0x0C] = 0x00; rawBytes[0x0D] = 0x80; // Tolerance: 0x8000
    rawBytes[0x14] = 0x00; rawBytes[0x15] = 0x80; // Nominal Value: 0x8000

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.TemperatureProbe,
      0x16,
      1,
      rawBytes,
      new List<string>()
    );

    // Act
    var result = T028_TemperatureProbeInformation.Decode(mockStructure);

    // Assert
    Assert.Equal(TemperatureProbeLocation.Unknown, result.Location);
    Assert.Equal(TemperatureProbeStatus.Unknown, result.Status);

    Assert.Null(result.MaximumValueCelsius);
    Assert.Null(result.MinimumValueCelsius);
    Assert.Null(result.ResolutionCelsius);
    Assert.Null(result.ToleranceCelsius);
    Assert.Null(result.NominalValueCelsius);
  }
}
