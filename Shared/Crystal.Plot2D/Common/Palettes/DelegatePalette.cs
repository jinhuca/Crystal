using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public sealed class DelegatePalette : PaletteBase {
  public DelegatePalette(Func<double, Color> _func) => func = _func ?? throw new ArgumentNullException(paramName: nameof(_func));
  private readonly Func<double, Color> func;
  public override Color GetColor(double t) => func(arg: t);
}
