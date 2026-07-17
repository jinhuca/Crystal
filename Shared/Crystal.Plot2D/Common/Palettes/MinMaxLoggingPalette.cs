using System.Diagnostics;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
/// Represents a palette that calculates minimal and maximal values of interpolation coefficient and every 100000 calls writes these values 
/// to DEBUG console.
/// </summary>
public class MinMaxLoggingPalette : DecoratorPaletteBase {
  private int counter;

  public double Min { get; set; } = double.MaxValue;
  public double Max { get; set; } = double.MinValue;

  public override Color GetColor(double t) {
    if(t < Min) {
      Min = t;
    }

    if(t > Max) {
      Max = t;
    }

    counter++;

    if(counter % 100000 == 0) {
      Debug.WriteLine(message: "Palette: Min = " + Min + ", max = " + Max);
    }

    return base.GetColor(t: t);
  }
}
