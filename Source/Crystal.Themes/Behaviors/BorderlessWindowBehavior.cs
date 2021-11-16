using System.Windows.Data;
using Crystal.Themes.Controls;
using Crystal.Themes.Windows.Shell;

namespace Crystal.Themes.Behaviors
{
  public class BorderlessWindowBehavior : WindowChromeBehavior
  {
    protected override void OnAttached()
    {
      BindingOperations.SetBinding(this, IgnoreTaskbarOnMaximizeProperty, new Binding { Path = new PropertyPath(CrystalWindow.IgnoreTaskbarOnMaximizeProperty), Source = AssociatedObject });
      BindingOperations.SetBinding(this, ResizeBorderThicknessProperty, new Binding { Path = new PropertyPath(CrystalWindow.ResizeBorderThicknessProperty), Source = AssociatedObject });
      BindingOperations.SetBinding(this, TryToBeFlickerFreeProperty, new Binding { Path = new PropertyPath(CrystalWindow.TryToBeFlickerFreeProperty), Source = AssociatedObject });
      BindingOperations.SetBinding(this, KeepBorderOnMaximizeProperty, new Binding { Path = new PropertyPath(CrystalWindow.KeepBorderOnMaximizeProperty), Source = AssociatedObject });
      BindingOperations.SetBinding(this, EnableMinimizeProperty, new Binding { Path = new PropertyPath(CrystalWindow.ShowMinButtonProperty), Source = AssociatedObject });
      BindingOperations.SetBinding(this, EnableMaxRestoreProperty, new Binding { Path = new PropertyPath(CrystalWindow.ShowMaxRestoreButtonProperty), Source = AssociatedObject });

      base.OnAttached();
    }

    protected override void OnDetaching()
    {
      BindingOperations.ClearBinding(this, IgnoreTaskbarOnMaximizeProperty);
      BindingOperations.ClearBinding(this, ResizeBorderThicknessProperty);
      BindingOperations.ClearBinding(this, TryToBeFlickerFreeProperty);
      BindingOperations.ClearBinding(this, KeepBorderOnMaximizeProperty);
      BindingOperations.ClearBinding(this, EnableMinimizeProperty);
      BindingOperations.ClearBinding(this, EnableMaxRestoreProperty);

      base.OnDetaching();
    }

    protected override void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
    {
      if (sender is CrystalWindow window)
      {
        window.SetIsHitTestVisibleInChromeProperty<UIElement>("PART_Icon");

        if (window.ResizeMode != ResizeMode.NoResize)
        {
          window.SetWindowChromeResizeGripDirection("WindowResizeGrip", ResizeGripDirection.BottomRight);
        }
      }
    }
  }
}