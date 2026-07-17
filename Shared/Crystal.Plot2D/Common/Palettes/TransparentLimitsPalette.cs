using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public class TransparentLimitsPalette : DecoratorPaletteBase {
  public TransparentLimitsPalette() { }

  public TransparentLimitsPalette(IPalette palette) : base(palette: palette) { }

  public override Color GetColor(double t) {
    if(t < 0 || t > 1) {
      return Colors.Transparent;
    }

    return Palette.GetColor(t: t);
  }
}
