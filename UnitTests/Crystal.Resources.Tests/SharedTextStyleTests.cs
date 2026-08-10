using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Crystal.Resources.Tests;

/// <summary>
/// Regression guard for the sibling-dictionary bug: TextStyles.xaml references brushes defined in
/// Palette.xaml. If the palette isn't merged into the text-style dictionary's own scope, the
/// Foreground setter resolves to {DependencyProperty.UnsetValue} — which does NOT fail at load,
/// only when layout measures a TextBlock using the style. These tests apply each shared style to a
/// real TextBlock and force a Measure pass, so a broken resource wiring fails here instead of
/// crashing the app on first render.
/// </summary>
public class SharedTextStyleTests {
  // Shared TextBlock style keys hoisted into TextStyles.xaml, each mapped to the palette brush key
  // its Foreground setter must resolve to. The expected color is looked up from the loaded palette
  // (see PaletteTests.Expected) rather than restated here, so a palette color change stays in one
  // place and can't silently drift from what these styles assert.
  public static readonly (string StyleKey, string PaletteKey)[] StyleForeground = {
    ("MetricValue", "ValueBrush"),
    ("MetricUnit", "UnitBrush"),
    ("MetricCaption", "LabelBrush"),
    ("HeaderSpec", "LabelBrush"),
    ("StatCaption", "LabelBrush"),
  };

  public static TheoryData<string, string> StyleForegroundData {
    get {
      var data = new TheoryData<string, string>();
      foreach (var (styleKey, paletteKey) in StyleForeground) data.Add(styleKey, paletteKey);
      return data;
    }
  }

  [Theory]
  [MemberData(nameof(StyleForegroundData))]
  public void SharedStyle_MeasuresWithoutUnsetForeground(string styleKey, string paletteKey) =>
    StaRunner.Run(() => {
      var generic = ResourceLoader.LoadGeneric();
      Assert.True(generic.Contains(styleKey), $"Generic.xaml is missing shared style '{styleKey}'.");
      var style = Assert.IsType<Style>(generic[styleKey]);

      var block = new TextBlock { Text = "0.00", Style = style };

      // The original crash surfaced in TextBlock.MeasureOverride, not at style assignment, so a
      // Measure pass is essential to reproduce it.
      block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

      var foreground = block.Foreground;
      Assert.NotEqual(DependencyProperty.UnsetValue, foreground);
      var brush = Assert.IsType<SolidColorBrush>(foreground);
      Assert.Equal(ExpectedColor(paletteKey), brush.Color);
    });

  [Fact]
  public void AllSharedStyles_TargetTextBlock() =>
    StaRunner.Run(() => {
      var generic = ResourceLoader.LoadGeneric();

      foreach (var (styleKey, _) in StyleForeground) {
        var style = Assert.IsType<Style>(generic[styleKey]);
        Assert.Equal(typeof(TextBlock), style.TargetType);
      }
    });

  // Resolves the palette brush the style should point at, so the expected color has a single source
  // of truth (PaletteTests locks each key's hex separately).
  private static Color ExpectedColor(string paletteKey) {
    var hex = PaletteTests.Expected.Single(e => e.Key == paletteKey).Hex;
    return (Color)ColorConverter.ConvertFromString(hex)!;
  }
}
