using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
///   Represents a base class for decorating palette, which wraps another palette and intercepts calls to it.
/// </summary>
public abstract class DecoratorPaletteBase : PaletteBase {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DecoratorPaletteBase"/> class.
  /// </summary>
  protected DecoratorPaletteBase() { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DecoratorPaletteBase"/> class.
  /// </summary>
  /// <param name="palette">
  ///   The palette.
  /// </param>
  protected DecoratorPaletteBase(IPalette palette) => Palette = palette;

  private IPalette palette;

  /// <summary>
  ///   Gets or sets the palette being decorated.
  /// </summary>
  /// <value>
  ///   The palette.
  /// </value>
  public IPalette Palette {
    get => palette;
    set {
      if(palette != null) {
        palette.Changed -= OnChildPaletteChanged;
      }

      palette = value ?? throw new ArgumentNullException(paramName: nameof(value));
      palette.Changed += OnChildPaletteChanged;
      RaiseChanged();
    }
  }

  private void OnChildPaletteChanged(object sender, EventArgs e) => RaiseChanged();

  /// <summary>
  ///   Gets the color by interpolation coefficient.
  /// </summary>
  /// <param name="t">
  ///   Interpolation coefficient, should belong to [0..1].
  /// </param>
  /// <returns>
  ///   Color.
  /// </returns>
  public override Color GetColor(double t) => palette.GetColor(t: t);
}
