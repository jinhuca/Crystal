using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

[ContentProperty(name: "ColorSteps")]
public sealed class UniformLinearPalette : IPalette, ISupportInitialize {
  public ObservableCollection<Color> Colors { get; private set; } = new();

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public List<PaletteColorStep> ColorSteps { get; } = new();

  [SuppressMessage(category: "Microsoft.Performance", checkId: "CA1811:AvoidUncalledPrivateCode")]
  private void RaiseChanged() => Changed.Raise(sender: this);
  public event EventHandler Changed;

  public UniformLinearPalette() { }

  public UniformLinearPalette(params Color[] colors) {
    ArgumentNullException.ThrowIfNull(colors);

    if(colors.Length < 2) {
      throw new ArgumentException(message: Strings.Exceptions.PaletteTooFewColors);
    }

    Colors = new ObservableCollection<Color>(collection: colors);
    FillPoints(size: colors.Length);
  }

  private void FillPoints(int size) {
    Points = new double[size];
    for(var i = 0; i < size; i++) {
      Points[i] = i / (double)(size - 1);
    }
  }
  [DefaultValue(value: true)]
  public bool IncreaseBrightness { get; set; } = true;

  public double[] Points { get; set; }
  public bool BeganInit { get; set; }

  public Color GetColor(double t) {
    Verify.AssertIsFinite(d: t);
    Verify.IsTrue(condition: 0 <= t && t <= 1);

    if(t <= 0) {
      return Colors[index: 0];
    }

    if(t >= 1) {
      return Colors[index: Colors.Count - 1];
    }
    var i = 0;
    while(Points[i] < t) {
      i++;
    }

    var ratio = (Points[i] - t) / (Points[i] - Points[i - 1]);

    Verify.IsTrue(condition: 0 <= ratio && ratio <= 1);

    var c0 = Colors[index: i - 1];
    var c1 = Colors[index: i];
    var res = Color.FromRgb(
      r: (byte)(c0.R * ratio + c1.R * (1 - ratio)),
      g: (byte)(c0.G * ratio + c1.G * (1 - ratio)),
      b: (byte)(c0.B * ratio + c1.B * (1 - ratio)));

    // Increasing saturation and brightness
    if(IncreaseBrightness) {
      var hsb = res.ToHsbColor();
      //hsb.Saturation = 0.5 * (1 + hsb.Saturation);
      hsb.Brightness = 0.5 * (1 + hsb.Brightness);
      return hsb.ToArgbColor();
    }

    return res;
  }

  #region ISupportInitialize Members

  public void BeginInit() => BeganInit = true;

  public void EndInit() {
    if(BeganInit) {
      Colors = new ObservableCollection<Color>(collection: ColorSteps.Select(selector: step => step.Color));
      FillPoints(size: Colors.Count);
    }
  }

  #endregion
}
