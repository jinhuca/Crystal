using Crystal.Plot2D.Common.Auxiliary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
/// Represents a palette with start and stop colors and intermediate colors with their custom offsets.
/// </summary>
[ContentProperty(name: "Steps")]
public sealed class LinearPalette : PaletteBase, ISupportInitialize {
  public ObservableCollection<LinearPaletteColorStep> Steps { get; } = new();

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public Color StartColor { get; } = Colors.White;

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public Color EndColor { get; } = Colors.Black;

  /// <summary>
  /// Initializes a new instance of the <see cref="LinearPalette"/> class.
  /// </summary>
  public LinearPalette() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="LinearPalette"/> class.
  /// </summary>
  /// <param name="startColor">The start color.</param>
  /// <param name="endColor">The end color.</param>
  /// <param name="steps">The steps.</param>
  public LinearPalette(Color startColor, Color endColor, params LinearPaletteColorStep[] steps) {
    Steps.Add(item: new LinearPaletteColorStep(color: startColor, offset: 0));
    if(steps != null) {
      Steps.AddMany(children: steps);
    }

    Steps.Add(item: new LinearPaletteColorStep(color: endColor, offset: 1));
  }

  #region IPalette Members

  /// <summary>
  /// Gets the color by interpolation coefficient.
  /// </summary>
  /// <param name="t">Interpolation coefficient, should belong to [0..1].</param>
  /// <returns>Color.</returns>
  public override Color GetColor(double t) {
    if(t < 0) {
      return Steps[index: 0].Color;
    }

    if(t > 1) {
      return Steps[index: Steps.Count - 1].Color;
    }

    var i = 0;
    double x = 0;
    while(x <= t) {
      x = Steps[index: i + 1].Offset;
      i++;
    }

    var ratio = (t - Steps[index: i - 1].Offset) / (Steps[index: i].Offset - Steps[index: i - 1].Offset);

    var c0 = Steps[index: i - 1].Color;
    var c1 = Steps[index: i].Color;

    return Color.FromRgb(
      r: (byte)((1 - ratio) * c0.R + ratio * c1.R),
      g: (byte)((1 - ratio) * c0.G + ratio * c1.G),
      b: (byte)((1 - ratio) * c0.B + ratio * c1.B));
  }

  #endregion

  #region ISupportInitialize Members

  void ISupportInitialize.BeginInit() {
  }

  void ISupportInitialize.EndInit() {
    if(Steps.Count == 0 || Steps[index: 0].Offset > 0) {
      Steps.Insert(index: 0, item: new LinearPaletteColorStep(color: StartColor, offset: 0));
    }

    if(Steps.Count == 0 || Steps[index: Steps.Count - 1].Offset < 1) {
      Steps.Add(item: new LinearPaletteColorStep(color: EndColor, offset: 1));
    }
  }

  #endregion
}
