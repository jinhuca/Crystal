using BiosModule.ViewModels;
using Xunit;

namespace BiosModule.Tests;

public class BoardReadingSeverityTests {
  [Fact]
  public void Null_rail_is_normal() =>
      Assert.Equal(ReadingSeverity.Normal, BoardReadingSeverity.Rail(null, 12f));

  [Fact]
  public void Null_cmos_is_normal() =>
      Assert.Equal(ReadingSeverity.Normal, BoardReadingSeverity.Cmos(null));

  [Theory]
  [InlineData(3.3f, 3.3f)]     // dead on
  [InlineData(3.4f, 3.3f)]     // ~+3%, in spec
  [InlineData(5.0f, 5f)]
  [InlineData(12.0f, 12f)]
  [InlineData(11.5f, 12f)]     // ~-4%
  public void Rail_within_five_percent_is_normal(float value, float nominal) =>
      Assert.Equal(ReadingSeverity.Normal, BoardReadingSeverity.Rail(value, nominal));

  [Theory]
  [InlineData(12.55f, 12f)]    // ~+4.6%, just inside the 5% band
  [InlineData(11.45f, 12f)]    // ~-4.6%
  public void Rail_just_inside_five_percent_stays_normal(float value, float nominal) =>
      Assert.Equal(ReadingSeverity.Normal, BoardReadingSeverity.Rail(value, nominal));

  [Theory]
  [InlineData(12.7f, 12f)]     // ~+5.8%
  [InlineData(11.3f, 12f)]     // ~-5.8%
  [InlineData(5.3f, 5f)]       // +6%
  [InlineData(13.1f, 12f)]     // ~+9.2%, just inside the 10% band
  public void Rail_between_five_and_ten_percent_is_warning(float value, float nominal) =>
      Assert.Equal(ReadingSeverity.Warning, BoardReadingSeverity.Rail(value, nominal));

  [Theory]
  [InlineData(13.3f, 12f)]     // ~+10.8%
  [InlineData(10.5f, 12f)]     // -12.5%
  [InlineData(0f, 12f)]        // rail collapsed
  [InlineData(5.6f, 5f)]       // +12%
  public void Rail_beyond_ten_percent_is_critical(float value, float nominal) =>
      Assert.Equal(ReadingSeverity.Critical, BoardReadingSeverity.Rail(value, nominal));

  [Theory]
  [InlineData(3.2f)]           // fresh cell
  [InlineData(3.0f)]           // nominal CR2032
  [InlineData(2.7f)]           // boundary → not < 2.7, still healthy
  public void Cmos_at_or_above_2_7_is_normal(float value) =>
      Assert.Equal(ReadingSeverity.Normal, BoardReadingSeverity.Cmos(value));

  [Theory]
  [InlineData(2.69f)]
  [InlineData(2.6f)]
  [InlineData(2.5f)]           // boundary → not < 2.5, still just warning
  public void Cmos_between_2_5_and_2_7_is_warning(float value) =>
      Assert.Equal(ReadingSeverity.Warning, BoardReadingSeverity.Cmos(value));

  [Theory]
  [InlineData(2.49f)]
  [InlineData(2.0f)]
  [InlineData(0f)]
  public void Cmos_below_2_5_is_critical(float value) =>
      Assert.Equal(ReadingSeverity.Critical, BoardReadingSeverity.Cmos(value));
}
