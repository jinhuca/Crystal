using System.Windows.Media;
using Xunit;

namespace Crystal.Resources.Tests;

/// <summary>
/// Locks the semantic-palette contract: every brush key views depend on must exist, be a
/// SolidColorBrush, and carry its documented color; the per-sensor banded gauge endpoints must
/// likewise exist as Color resources of their documented value (they feed PerformanceGraph's
/// BandStartColor/BandEndColor, which are Color, not Brush). A rename or dropped key fails here
/// loudly instead of silently blanking a view at runtime.
/// </summary>
public class PaletteTests {
  // Key -> expected hex, mirroring Palette.xaml. Kept explicit so a color change is a deliberate
  // test edit, not a silent drift.
  public static readonly (string Key, string Hex)[] Expected = {
    ("LabelBrush", "#8A94A0"),
    ("ValueBrush", "#E6E6E6"),
    ("UnitBrush", "#C0C0C0"),
    ("TextBrush", "#FFFFFF"),
    ("TextPrimaryBrush", "#DDDDDD"),
    ("TextMutedBrush", "#C8C8C8"),
    ("SpecValueBrush", "#C8D0D8"),
    ("AccentBrush", "#3E9BE8"),
    ("AccentSelectionBrush", "#17324B"),
    ("AccentHoverBrush", "#1C2530"),
    ("OkBrush", "#3BD15A"),
    ("WarningBrush", "#E8A33E"),
    ("DangerBrush", "#C43C3C"),
    ("PanelBackgroundBrush", "#101317"),
    ("SurfaceBackgroundBrush", "#161B22"),
    ("DividerBrush", "#2A2A2A"),
    ("BorderBrush", "#3A3A3A"),
    ("UsageBarUsedBrush", "#2E7DD1"),
    ("UsageBarFreeBrush", "#3A5068"),
    ("UsageBarTrackBrush", "#12233A"),
    ("UsageBarBorderBrush", "#2E4A6A"),

    // Per-sensor banded ramps (band 1 = lowest reading … band 9 = highest) used by the gauge/graph
    // tiles to color a reading by where it falls in its range.
    ("ClockAccentBandedBrush1", "#FFF5F5F5"),
    ("ClockAccentBandedBrush2", "#FFE8F4E8"),
    ("ClockAccentBandedBrush3", "#FFDCF3DC"),
    ("ClockAccentBandedBrush4", "#FFCFF2CF"),
    ("ClockAccentBandedBrush5", "#FFC2F2C2"),
    ("ClockAccentBandedBrush6", "#FFB6F1B6"),
    ("ClockAccentBandedBrush7", "#FFA9F0A9"),
    ("ClockAccentBandedBrush8", "#FF9DEF9D"),
    ("ClockAccentBandedBrush9", "#FF90EE90"),

    ("VoltageAccentBandedBrush1", "#FFF5F5F5"),
    ("VoltageAccentBandedBrush2", "#FFDDDDEA"),
    ("VoltageAccentBandedBrush3", "#FFC4C5DE"),
    ("VoltageAccentBandedBrush4", "#FFACADD3"),
    ("VoltageAccentBandedBrush5", "#FF9494C8"),
    ("VoltageAccentBandedBrush6", "#FF7C7CBC"),
    ("VoltageAccentBandedBrush7", "#FF6464B1"),
    ("VoltageAccentBandedBrush8", "#FF4B4CA5"),
    ("VoltageAccentBandedBrush9", "#FF33349A"),

    ("TemperatureAccentBandedBrush1", "#FFF5F5F5"),
    ("TemperatureAccentBandedBrush2", "#FFF6DDD9"),
    ("TemperatureAccentBandedBrush3", "#FFF8C4BE"),
    ("TemperatureAccentBandedBrush4", "#FFF9ACA2"),
    ("TemperatureAccentBandedBrush5", "#FFFA9486"),
    ("TemperatureAccentBandedBrush6", "#FFFB7B6A"),
    ("TemperatureAccentBandedBrush7", "#FFFC634E"),
    ("TemperatureAccentBandedBrush8", "#FFFE4A33"),
    ("TemperatureAccentBandedBrush9", "#FFFF3217"),

    ("PowerAccentBandedBrush1", "#FFF5F5F5"),
    ("PowerAccentBandedBrush2", "#FFF4EAE2"),
    ("PowerAccentBandedBrush3", "#FFF4DECE"),
    ("PowerAccentBandedBrush4", "#FFF3D3BA"),
    ("PowerAccentBandedBrush5", "#FFF2C8A7"),
    ("PowerAccentBandedBrush6", "#FFF2BD94"),
    ("PowerAccentBandedBrush7", "#FFF1B280"),
    ("PowerAccentBandedBrush8", "#FFF1A66C"),
    ("PowerAccentBandedBrush9", "#FFF09B59"),
  };

  // Per-sensor banded gauge endpoints, exposed as Color (not Brush) resources because they feed
  // PerformanceGraph.BandStartColor / BandEndColor (Color-typed DPs). Band 1 = lowest reading, band
  // 9 = highest; the CPU family ramps green→red and the GPU family blue→red. Kept explicit for the
  // same reason as the brushes above: a color change is a deliberate test edit, not silent drift.
  public static readonly (string Key, string Hex)[] ExpectedColors = {
    ("CpuPowerAccentBandedColor1", "#FF90EE90"),
    ("CpuPowerAccentBandedColor9", "#FFFF4556"),
    ("CpuTemperatureAccentBandedColor1", "#FF90EE90"),
    ("CpuTemperatureAccentBandedColor9", "#FFFF4556"),
    ("CpuClockAccentBandedColor1", "#FF90EE90"),
    ("CpuClockAccentBandedColor9", "#FFFF4556"),
    ("CpuVoltAccentBandedColor1", "#FF90EE90"),
    ("CpuVoltAccentBandedColor9", "#FFFF4556"),
    ("CpuFanAccentBandedColor1", "#FF90EE90"),
    ("CpuFanAccentBandedColor9", "#FFFF4556"),

    ("GpuPowerBandedColor1", "#FF2E7DD1"),
    ("GpuPowerBandedColor9", "#FFFF2D2D"),
    ("GpuTemperatureBandedColor1", "#FF2E7DD1"),
    ("GpuTemperatureBandedColor9", "#FFFF2D2D"),
    ("GpuHotspotBandedColor1", "#FF2E7DD1"),
    ("GpuHotspotBandedColor9", "#FFFF2D2D"),
    ("GpuMemoryBandedColor1", "#FF2E7DD1"),
    ("GpuMemoryBandedColor9", "#FFFF2D2D"),
    ("GpuClockBandedColor1", "#FF2E7DD1"),
    ("GpuClockBandedColor9", "#FFFF2D2D"),
    ("Gpu3DBandedColor1", "#FF2E7DD1"),
    ("Gpu3DBandedColor9", "#FFFF2D2D"),
    ("GpuPCIeRxBandedColor1", "#FF2E7DD1"),
    ("GpuPCIeRxBandedColor9", "#FFFF2D2D"),
    ("GpuPCIeTxBandedColor1", "#FF2E7DD1"),
    ("GpuPCIeTxBandedColor9", "#FFFF2D2D"),
    ("GpuFanBandedColor1", "#FF2E7DD1"),
    ("GpuFanBandedColor9", "#FFFF2D2D"),
    ("GpuVoltBandedColor1", "#FF2E7DD1"),
    ("GpuVoltBandedColor9", "#FFFF2D2D"),
    ("GpuUtilizationBandedColor1", "#FF2E7DD1"),
    ("GpuUtilizationBandedColor9", "#FFFF2D2D"),
  };

  public static TheoryData<string, string> ExpectedData {
    get {
      var data = new TheoryData<string, string>();
      foreach (var (key, hex) in Expected) data.Add(key, hex);
      return data;
    }
  }

  public static TheoryData<string, string> ExpectedColorData {
    get {
      var data = new TheoryData<string, string>();
      foreach (var (key, hex) in ExpectedColors) data.Add(key, hex);
      return data;
    }
  }

  [Theory]
  [MemberData(nameof(ExpectedData))]
  public void Palette_Key_IsSolidColorBrushOfExpectedColor(string key, string hex) =>
    StaRunner.Run(() => {
      var palette = ResourceLoader.LoadPalette();

      Assert.True(palette.Contains(key), $"Palette is missing key '{key}'.");
      var brush = Assert.IsType<SolidColorBrush>(palette[key]);
      Assert.Equal((Color)ColorConverter.ConvertFromString(hex)!, brush.Color);
    });

  [Theory]
  [MemberData(nameof(ExpectedColorData))]
  public void Palette_ColorKey_IsColorOfExpectedValue(string key, string hex) =>
    StaRunner.Run(() => {
      var palette = ResourceLoader.LoadPalette();

      Assert.True(palette.Contains(key), $"Palette is missing key '{key}'.");
      var color = Assert.IsType<Color>(palette[key]);
      Assert.Equal((Color)ColorConverter.ConvertFromString(hex)!, color);
    });

  [Fact]
  public void Palette_ContainsNoUnexpectedExtraKeys() =>
    StaRunner.Run(() => {
      var palette = ResourceLoader.LoadPalette();
      var expectedKeys = Expected.Select(e => e.Key)
          .Concat(ExpectedColors.Select(e => e.Key))
          .ToHashSet();

      var actualKeys = palette.Keys.Cast<object>().Select(k => k.ToString()!).ToHashSet();

      Assert.Equal(expectedKeys.OrderBy(k => k), actualKeys.OrderBy(k => k));
    });
}
