using System.Windows.Media;
using Crystal.Controls.MeterGauges.Themes;
using Xunit;

namespace Crystal.Controls.Tests.MeterGauges;

public class GaugeThemesTests {
  [Fact]
  public void FromAccent_SetsActiveBrushToAccentColor() {
    var accent = Color.FromRgb(0x12, 0x34, 0x56);

    GaugeTheme theme = GaugeThemes.FromAccent(accent);

    var active = Assert.IsType<SolidColorBrush>(theme.ActiveBrush);
    Assert.Equal(accent, active.Color);
  }

  [Fact]
  public void FromAccent_PopulatesBackgroundAndInactive() {
    GaugeTheme theme = GaugeThemes.FromAccent(Color.FromRgb(1, 2, 3));

    Assert.NotNull(theme.GaugeBackground);
    Assert.NotNull(theme.InactiveBrush);
  }

  [Fact]
  public void FromAccent_ReturnsFrozenBrushes() {
    GaugeTheme theme = GaugeThemes.FromAccent(Color.FromRgb(1, 2, 3));

    Assert.True(theme.ActiveBrush!.IsFrozen);
    Assert.True(theme.InactiveBrush!.IsFrozen);
    Assert.True(theme.GaugeBackground!.IsFrozen);
  }

  [Fact]
  public void Emerald_UsesExpectedAccent() {
    GaugeTheme theme = GaugeThemes.Emerald();

    var active = Assert.IsType<SolidColorBrush>(theme.ActiveBrush);
    Assert.Equal(Color.FromRgb(0x3B, 0xD1, 0x5A), active.Color);
  }

  [Fact]
  public void Presets_ProduceDistinctAccents() {
    var rose = (SolidColorBrush)GaugeThemes.Rose().ActiveBrush!;
    var amber = (SolidColorBrush)GaugeThemes.Amber().ActiveBrush!;
    var sky = (SolidColorBrush)GaugeThemes.Sky().ActiveBrush!;
    var emerald = (SolidColorBrush)GaugeThemes.Emerald().ActiveBrush!;

    var colors = new[] { rose.Color, amber.Color, sky.Color, emerald.Color };
    Assert.Equal(colors.Length, colors.Distinct().Count());
  }
}
