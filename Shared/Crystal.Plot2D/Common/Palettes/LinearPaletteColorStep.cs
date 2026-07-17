using System.Diagnostics;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
/// Represents a color step with its offset in limits [0..1].
/// </summary>
[DebuggerDisplay(value: "Color={Color}, Offset={Offset}")]
public sealed class LinearPaletteColorStep {
  /// <summary>
  /// Initializes a new instance of the <see cref="LinearPaletteColorStep"/> class.
  /// </summary>
  public LinearPaletteColorStep() { }
  /// <summary>
  /// Initializes a new instance of the <see cref="LinearPaletteColorStep"/> class.
  /// </summary>
  /// <param name="color">The color.</param>
  /// <param name="offset">The offset.</param>
  public LinearPaletteColorStep(Color color, double offset) {
    Color = color;
    Offset = offset;
  }

  /// <summary>
  /// Gets or sets the color.
  /// </summary>
  /// <value>The color.</value>
  public Color Color { get; set; }
  /// <summary>
  /// Gets or sets the offset.
  /// </summary>
  /// <value>The offset.</value>
  public double Offset { get; set; }
}
