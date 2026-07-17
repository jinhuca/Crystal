using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
/// Contains some predefined linear palettes.
/// </summary>
public static class LinearPalettes {
  private const double geoHeight = 8845 + 400;

  /// <summary>
  /// Gets the palette for geo height map visualization.
  /// </summary>
  /// <value>The geo heights palette.</value>
  public static LinearPalette GeoHeightsPalette { get; } = new(
    startColor: Color.FromRgb(r: 1, g: 99, b: 69), endColor: Colors.White,
    steps: new[]
    {
      new LinearPaletteColorStep(color: Color.FromRgb(r: 28, g: 128, b: 52), offset: (50 + 400) / geoHeight),
      new LinearPaletteColorStep(color: Color.FromRgb(r: 229, g: 209, b: 119), offset: (200 + 400) / geoHeight),
      new LinearPaletteColorStep(color: Color.FromRgb(r: 160, g: 66, b: 1), offset: (1000 + 400) / geoHeight),
      new LinearPaletteColorStep(color: Color.FromRgb(r: 129, g: 32, b: 32), offset: (2000 + 400) / geoHeight),
      new LinearPaletteColorStep(color: Color.FromRgb(r: 119, g: 119, b: 119), offset: (4000 + 400) / geoHeight),
      new LinearPaletteColorStep(color: Color.FromRgb(r: 244, g: 244, b: 244), offset: (6000 + 400) / geoHeight)
    });
}
