using System.Windows.Media;
using Xunit;

namespace Crystal.Resources.Tests;

/// <summary>
/// Locks the semantic-palette contract: every brush key views depend on must exist, be a
/// SolidColorBrush, and carry its documented color. A rename or dropped key fails here loudly
/// instead of silently blanking a view at runtime.
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
  };

  public static TheoryData<string, string> ExpectedData {
    get {
      var data = new TheoryData<string, string>();
      foreach (var (key, hex) in Expected) data.Add(key, hex);
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

  [Fact]
  public void Palette_ContainsNoUnexpectedExtraKeys() =>
    StaRunner.Run(() => {
      var palette = ResourceLoader.LoadPalette();
      var expectedKeys = Expected.Select(e => e.Key).ToHashSet();

      var actualKeys = palette.Keys.Cast<object>().Select(k => k.ToString()!).ToHashSet();

      Assert.Equal(expectedKeys.OrderBy(k => k), actualKeys.OrderBy(k => k));
    });
}
