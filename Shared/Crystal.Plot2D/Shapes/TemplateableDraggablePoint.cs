using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Crystal.Plot2D.Shapes;

public class TemplateableDraggablePoint : DraggablePoint {
  private readonly Control marker = new() { Focusable = false };

  public TemplateableDraggablePoint() {
    marker.SetBinding(dp: TemplateProperty, binding: new Binding { Source = this, Path = new PropertyPath(path: "MarkerTemplate") });
    Content = marker;
  }

  public ControlTemplate MarkerTemplate {
    get => (ControlTemplate)GetValue(dp: MarkerTemplateProperty);
    set => SetValue(dp: MarkerTemplateProperty, value: value);
  }

  public static readonly DependencyProperty MarkerTemplateProperty = DependencyProperty.Register(
    name: nameof(MarkerTemplate),
    propertyType: typeof(ControlTemplate),
    ownerType: typeof(TemplateableDraggablePoint),
    typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));
}
