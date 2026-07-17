using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public class PowerPalette : DecoratorPaletteBase {
  public PowerPalette() { }

  public PowerPalette(IPalette palette) : base(palette: palette) { }

  public override Color GetColor(double t) {
    // todo create a property for power base setting
    return base.GetColor(t: Math.Pow(x: t, y: 0.1));
  }
}
