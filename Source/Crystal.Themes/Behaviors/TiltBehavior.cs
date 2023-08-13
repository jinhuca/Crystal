using Crystal.Themes.Theming;

namespace Crystal.Themes.Behaviors;

public class TiltBehavior : Behavior<FrameworkElement>
{
  private bool isPressed;
  private Thickness originalMargin;
  private Panel? originalPanel;
  private Size originalSize;
  private FrameworkElement? attachedElement;
  private Point current = new Point(-99, -99);
  private int times = -1;

  /// <summary>Identifies the <see cref="KeepDragging"/> dependency property.</summary>
  public static readonly DependencyProperty KeepDraggingProperty
    = DependencyProperty.Register(
      nameof(KeepDragging),
      typeof(bool),
      typeof(TiltBehavior),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  public bool KeepDragging
  {
    get => (bool)GetValue(KeepDraggingProperty);
    set => SetValue(KeepDraggingProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="TiltFactor"/> dependency property.</summary>
  public static readonly DependencyProperty TiltFactorProperty
    = DependencyProperty.Register(
      nameof(TiltFactor),
      typeof(int),
      typeof(TiltBehavior),
      new PropertyMetadata(20));

  public int TiltFactor
  {
    get => (int)GetValue(TiltFactorProperty);
    set => SetValue(TiltFactorProperty, value);
  }

  public Planerator? RotatorParent { get; private set; }

  protected override void OnAttached()
  {
    base.OnAttached();

    attachedElement = AssociatedObject;
    if (attachedElement is ListBox)
    {
      return;
    }

    if (attachedElement is Panel panel)
    {
      panel.Loaded += (sl, el) =>
      {
        var elements = panel.Children.OfType<UIElement>().ToList();

        elements.ForEach(element =>
          Interaction.GetBehaviors(element).Add(
            new TiltBehavior
            {
              KeepDragging = KeepDragging,
              TiltFactor = TiltFactor
            }));
      };

      return;
    }

    originalPanel = attachedElement.Parent as Panel ?? attachedElement.TryFindParent<Panel>();

    originalMargin = attachedElement.Margin;
    originalSize = new Size(attachedElement.Width, attachedElement.Height);
    double left = Canvas.GetLeft(attachedElement);
    double right = Canvas.GetRight(attachedElement);
    double top = Canvas.GetTop(attachedElement);
    double bottom = Canvas.GetBottom(attachedElement);
    int z = Panel.GetZIndex(attachedElement);
    VerticalAlignment va = attachedElement.VerticalAlignment;
    HorizontalAlignment ha = attachedElement.HorizontalAlignment;

    RotatorParent = new Planerator
    {
      Margin = originalMargin,
      Width = originalSize.Width,
      Height = originalSize.Height,
      VerticalAlignment = va,
      HorizontalAlignment = ha
    };

    RotatorParent.SetValue(Canvas.LeftProperty, left);
    RotatorParent.SetValue(Canvas.RightProperty, right);
    RotatorParent.SetValue(Canvas.TopProperty, top);
    RotatorParent.SetValue(Canvas.BottomProperty, bottom);
    RotatorParent.SetValue(Panel.ZIndexProperty, z);

    originalPanel?.Children.Remove(attachedElement);
    attachedElement.Margin = new Thickness();
    attachedElement.Width = double.NaN;
    attachedElement.Height = double.NaN;

    originalPanel?.Children.Add(RotatorParent);
    RotatorParent.Child = attachedElement;

    CompositionTarget.Rendering += CompositionTargetRendering;
    ThemeManager.Current.ThemeChanged += HandleThemeManagerThemeChanged;
  }

  private void HandleThemeManagerThemeChanged(object? sender, ThemeChangedEventArgs e)
  {
    this.Invoke(() => { RotatorParent?.Refresh(); });
  }

  protected override void OnDetaching()
  {
    CompositionTarget.Rendering -= CompositionTargetRendering;
    ThemeManager.Current.ThemeChanged -= HandleThemeManagerThemeChanged;

    base.OnDetaching();
  }

  private void CompositionTargetRendering(object? sender, EventArgs e)
  {
    if (RotatorParent is null
        || attachedElement is null)
    {
      return;
    }

    if (KeepDragging)
    {
      current = Mouse.GetPosition(RotatorParent.Child);
      if (Mouse.LeftButton == MouseButtonState.Pressed)
      {
        if (current.X > 0 && current.X < (attachedElement).ActualWidth && current.Y > 0 && current.Y < (attachedElement).ActualHeight)
        {
          RotatorParent.RotationY = -1 * TiltFactor + current.X * 2 * TiltFactor / (attachedElement).ActualWidth;
          RotatorParent.RotationX = -1 * TiltFactor + current.Y * 2 * TiltFactor / (attachedElement).ActualHeight;
        }
      }
      else
      {
        RotatorParent.RotationY = RotatorParent.RotationY - 5 < 0 ? 0 : RotatorParent.RotationY - 5;
        RotatorParent.RotationX = RotatorParent.RotationX - 5 < 0 ? 0 : RotatorParent.RotationX - 5;
      }
    }
    else
    {
      if (Mouse.LeftButton == MouseButtonState.Pressed)
      {
        if (!isPressed)
        {
          current = Mouse.GetPosition(RotatorParent.Child);
          if (current.X > 0 && current.X < (attachedElement).ActualWidth && current.Y > 0 && current.Y < (attachedElement).ActualHeight)
          {
            RotatorParent.RotationY = -1 * TiltFactor + current.X * 2 * TiltFactor / (attachedElement).ActualWidth;
            RotatorParent.RotationX = -1 * TiltFactor + current.Y * 2 * TiltFactor / (attachedElement).ActualHeight;
          }

          isPressed = true;
        }

        if (times == 7)
        {
          RotatorParent.RotationY = RotatorParent.RotationY - 5 < 0 ? 0 : RotatorParent.RotationY - 5;
          RotatorParent.RotationX = RotatorParent.RotationX - 5 < 0 ? 0 : RotatorParent.RotationX - 5;
        }
        else if (times < 7)
        {
          times++;
        }
      }
      else
      {
        isPressed = false;
        times = -1;
        RotatorParent.RotationY = RotatorParent.RotationY - 5 < 0 ? 0 : RotatorParent.RotationY - 5;
        RotatorParent.RotationX = RotatorParent.RotationX - 5 < 0 ? 0 : RotatorParent.RotationX - 5;
      }
    }
  }
}