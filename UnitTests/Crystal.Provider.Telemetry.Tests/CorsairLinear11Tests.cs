using Crystal.Provider.Telemetry.Hardware.Psu.Corsair;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

/// <summary>
/// Covers the PMBus LINEAR11 decode in the Corsair PSU <c>UsbApi</c>: a 5-bit signed exponent
/// (top 5 bits) scaling an 11-bit mantissa (low 11 bits). Vectors are chosen with mantissa bit 10
/// clear so the value equals <c>mantissa * 2^exponent</c> unambiguously.
/// </summary>
public class CorsairLinear11Tests {
  [Theory]
  [InlineData(0x0000, 0f)]      // exp 0, mant 0
  [InlineData(0x0001, 1f)]      // exp 0, mant 1
  [InlineData(0x0002, 2f)]      // exp 0, mant 2
  [InlineData(0x000A, 10f)]     // exp 0, mant 10
  [InlineData(0x0801, 2f)]      // exp +1, mant 1 -> 1 * 2^1
  [InlineData(0x1001, 4f)]      // exp +2, mant 1 -> 1 * 2^2
  [InlineData(0x7801, 32768f)]  // exp +15, mant 1 -> 1 * 2^15
  [InlineData(0xF801, 0.5f)]    // exp -1, mant 1 -> 1 * 2^-1
  public void Linear11ToFloat32_DecodesExponentAndMantissa(int raw, float expected) {
    Assert.Equal(expected, UsbApi.Linear11ToFloat32((ushort)raw));
  }
}
