using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

public sealed class HsbPalette : IPalette {
  /// <summary>
  /// Initializes a new instance of the <see cref="HsbPalette"/> class.
  /// </summary>
  public HsbPalette() { }

  private double start;
  [DefaultValue(value: 0.0)]
  public double Start {
    get => start;
    set {
      if(start != value) {
        start = value;
        Changed.Raise(sender: this);
      }
    }
  }

  private double width = 360;
  [DefaultValue(value: 360.0)]
  public double Width {
    get => width;
    set {
      if(width != value) {
        width = value;
        Changed.Raise(sender: this);
      }
    }
  }

  #region IPalette Members

  public Color GetColor(double t) {
    Verify.IsTrue(condition: 0 <= t && t <= 1);
    return new HsbColor(hue: start + t * width, saturation: 1, brightness: 1).ToArgbColor();
  }

  public event EventHandler Changed;

  #endregion
}
