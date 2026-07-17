using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public static class UniformLinearPalettes {
  static UniformLinearPalettes() {
    BlackAndWhitePalette.IncreaseBrightness = false;
    RedGreenBluePalette.IncreaseBrightness = false;
    BlueOrangePalette.IncreaseBrightness = false;
  }

  private static UniformLinearPalette BlackAndWhitePalette { get; } = new(colors: new[]
    { Colors.Black, Colors.White });

  private static UniformLinearPalette RedGreenBluePalette { get; } = new(colors: new[]
    { Colors.Blue, Color.FromRgb(r: 0, g: 255, b: 0), Colors.Red });

  private static UniformLinearPalette BlueOrangePalette { get; } = new(colors: new[]
    { Colors.Blue, Colors.Cyan, Colors.Yellow, Colors.Orange });
}
