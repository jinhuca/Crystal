using Crystal.Provider.Telemetry.Hardware.Cpu;
using System.Text;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

/// <summary>Covers the pure bit/string helpers in <see cref="CpuId"/> (the CPUID query itself is hardware).</summary>
public class CpuIdHelperTests {
  [Theory]
  [InlineData(0, 0u)]     // guard: x <= 0 -> 0
  [InlineData(-5, 0u)]
  [InlineData(1, 0u)]     // ceil(log2(1)) = 0
  [InlineData(2, 1u)]
  [InlineData(3, 2u)]
  [InlineData(4, 2u)]
  [InlineData(5, 3u)]
  [InlineData(8, 3u)]
  [InlineData(16, 4u)]
  [InlineData(64, 6u)]
  public void NextLog2_ReturnsCeilingLog2(long x, uint expected) {
    Assert.Equal(expected, CpuId.NextLog2(x));
  }

  [Fact]
  public void AppendRegister_UnpacksLittleEndianAsciiChars() {
    // "Genu" packed little-endian: 'G'=0x47 (low byte) .. 'u'=0x75 (high byte).
    uint packed = 'G' | ((uint)'e' << 8) | ((uint)'n' << 16) | ((uint)'u' << 24);
    var sb = new StringBuilder();

    CpuId.AppendRegister(sb, packed);

    Assert.Equal("Genu", sb.ToString());
  }

  [Fact]
  public void AppendRegister_ReconstructsFullVendorString() {
    // The three EBX/EDX/ECX registers of the Intel vendor string "GenuineIntel".
    var sb = new StringBuilder();
    CpuId.AppendRegister(sb, 0x756E6547); // "Genu"
    CpuId.AppendRegister(sb, 0x49656E69); // "ineI"
    CpuId.AppendRegister(sb, 0x6C65746E); // "ntel"

    Assert.Equal("GenuineIntel", sb.ToString());
  }
}
