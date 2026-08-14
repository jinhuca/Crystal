using Crystal.Provider.Smbios;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System;
using System.Collections.Generic;
using Xunit;

namespace Crystal.Provider.Smbios.Tests;

public class VoltageProbeInformationTests {
  [Fact]
  public void Decode_ValidVoltageProbe_ParsesFieldsAndSplitsStatusByte() {
    // Arrange: Create a complete Type 26 structure footprint matching SMBIOS specifications
    // DSP0134 §7.27: LocationAndStatus @0x05, Maximum @0x06, Minimum @0x08,
    // Resolution @0x0A, Tolerance @0x0C, Accuracy @0x0E, OEM DWORD @0x10,
    // Nominal @0x14. Structure length = 0x16.
    byte[] rawBytes = new byte[0x16];
    rawBytes[0x00] = 0x1A; // Type 26
    rawBytes[0x01] = 0x16; // Length 22
    rawBytes[0x04] = 1;    // Description string index 1

    // LocationAndStatusRaw calculation:
    // Status = OK (0x03) -> 0x03 << 5 = 0x60
    // Location = Processor (0x03) -> 0x03
    // Combined = 0x60 | 0x03 = 0x63
    rawBytes[0x05] = 0x63;
    rawBytes[0x06] = 0x46; rawBytes[0x07] = 0x05; // Max Value: 1350 mV (0x0546)
    rawBytes[0x08] = 0x84; rawBytes[0x09] = 0x03; // Min Value: 900 mV (0x0384)
    rawBytes[0x0A] = 0x0A; rawBytes[0x0B] = 0x00; // Resolution: 10 mV (0x000A)
    rawBytes[0x0C] = 0x32; rawBytes[0x0D] = 0x00; // Tolerance: 50 mV (0x0032)
    rawBytes[0x0E] = 0x5F; rawBytes[0x0F] = 0x00; // Accuracy: 95% mapping integer representation (0x005F)
    rawBytes[0x10] = 0x00; rawBytes[0x11] = 0x00;
    rawBytes[0x12] = 0x00; rawBytes[0x13] = 0x00; // OEM Defined
    rawBytes[0x14] = 0xB0; rawBytes[0x15] = 0x04; // Nominal Value: 1200 mV (0x04B0)

    var stringTable = new List<string> {
      "+1.2V CPU Vcore Probe"
    };

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.VoltageProbe,
      0x16,
      1,
      rawBytes,
      stringTable
    );

    // Act
    var result = T026_VoltageProbeInformation.Decode(mockStructure);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("+1.2V CPU Vcore Probe", result.Description);

    Assert.Equal(0x63, result.LocationAndStatusRaw);
    Assert.Equal(VoltageProbeLocation.Processor, result.Location);
    Assert.Equal(VoltageProbeStatus.OK, result.Status);

    Assert.Equal(1350, result.MaximumValueMillivolts);
    Assert.True(result.IsMaxValuedIdentifiable);
    Assert.Equal(900, result.MinimumValueMillivolts);
    Assert.True(result.IsMinValuedIdentifiable);

    Assert.Equal(10, result.ResolutionMillivolts);
    Assert.Equal(50, result.ToleranceMillivolts);
    Assert.Equal(95u, result.Accuracy);
    Assert.Equal(1200, result.NominalValueMillivolts);
    Assert.True(result.IsNominalValuedIdentifiable);
  }

  [Fact]
  public void Decode_UnknownTelemetryThresholds_EvaluatesMarkersCorrectly() {
    // Arrange: Create a minimal Type 26 structure layout with unknown value patterns (0x8000)
    byte[] rawBytes = new byte[0x16];
    rawBytes[0x05] = 0x42; // Status = Unknown (0x02 << 5 = 0x40), Location = Unknown (0x02) -> 0x42
    rawBytes[0x06] = 0x00; rawBytes[0x07] = 0x80; // Max Value: 0x8000 (Unknown)
    rawBytes[0x08] = 0x00; rawBytes[0x09] = 0x80; // Min Value: 0x8000 (Unknown)
    rawBytes[0x14] = 0x00; rawBytes[0x15] = 0x80; // Nominal Value: 0x8000 (Unknown)

    var mockStructure = new MockSmbiosRawStructure(
      SmbiosStructureType.VoltageProbe,
      0x16,
      1,
      rawBytes,
      new List<string>()
    );

    // Act
    var result = T026_VoltageProbeInformation.Decode(mockStructure);

    // Assert
    Assert.Equal(VoltageProbeLocation.Unknown, result.Location);
    Assert.Equal(VoltageProbeStatus.Unknown, result.Status);

    Assert.Equal(0x8000, result.MaximumValueMillivolts);
    Assert.False(result.IsMaxValuedIdentifiable);

    Assert.Equal(0x8000, result.MinimumValueMillivolts);
    Assert.False(result.IsMinValuedIdentifiable);

    Assert.Equal(0x8000, result.NominalValueMillivolts);
    Assert.False(result.IsNominalValuedIdentifiable);
  }
}
