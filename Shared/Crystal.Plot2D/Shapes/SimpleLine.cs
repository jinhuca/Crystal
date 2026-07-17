using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Represents simple line bound to viewport coordinates.
/// </summary>
public abstract class SimpleLine : ViewportShape {
  /// <summary>
  /// Initializes a new instance of the <see cref="SimpleLine"/> class.
  /// </summary>
  protected SimpleLine() {
  }

  /// <summary>
  /// Gets or sets the value of line - e.g., its horizontal or vertical coordinate.
  /// </summary>
  /// <value>The value.</value>
  public double Value {
    get => (double)GetValue(dp: ValueProperty);
    set => SetValue(dp: ValueProperty, value: value);
  }

  /// <summary>
  /// Identifies Value dependency property.
  /// </summary>
  public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
      name: nameof(Value),
      propertyType: typeof(double),
      ownerType: typeof(SimpleLine),
      typeMetadata: new PropertyMetadata(defaultValue: 0.0, propertyChangedCallback: OnValueChanged));

  private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var line_ = (SimpleLine)d;
    line_.OnValueChanged();
  }

  private void OnValueChanged() {
    UpdateUIRepresentation();
  }

  private readonly LineGeometry lineGeometry = new();
  protected LineGeometry LineGeometry => lineGeometry;

  protected override Geometry DefiningGeometry => lineGeometry;
}
