using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public sealed class PaletteColorStep {
  public PaletteColorStep(Color color) {
    Color = color;
  }

  public Color Color { get; }
}
