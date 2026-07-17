using Crystal.Plot2D.Transforms;
using System.Windows;
using System.Windows.Input;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Represents a simple draggable point with position bound to point in viewport coordinates, which allows to drag itself by mouse.
/// </summary>
public partial class DraggablePoint {
  /// <summary>
  /// Initializes a new instance of the <see cref="DraggablePoint"/> class.
  /// </summary>
  public DraggablePoint() {
    InitializeComponent();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DraggablePoint"/> class.
  /// </summary>
  /// <param name="position">The position of DraggablePoint.</param>
  public DraggablePoint(Point position) : this() { Position = position; }

  private bool dragging;
  private Point dragStart;
  private Vector shift;

  protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
    if(Plotter == null) {
      return;
    }

    dragStart = e.GetPosition(relativeTo: Plotter.ViewportPanel).ScreenToData(transform: Plotter.Viewport.Transform);
    shift = Position - dragStart;
    dragging = true;
  }

  protected override void OnMouseLeave(MouseEventArgs e) {
    ReleaseMouseCapture();
    dragging = false;
  }

  protected override void OnMouseMove(MouseEventArgs e) {
    if(!dragging) {
      if(IsMouseCaptured) {
        ReleaseMouseCapture();
      }

      return;
    }

    if(!IsMouseCaptured) {
      CaptureMouse();
    }

    var mouseInData = e.GetPosition(relativeTo: Plotter.ViewportPanel).ScreenToData(transform: Plotter.Viewport.Transform);

    if(mouseInData != dragStart) {
      Position = mouseInData + shift;
      e.Handled = true;
    }
  }

  protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
    if(dragging) {
      dragging = false;
      if(IsMouseCaptured) {
        ReleaseMouseCapture();
        e.Handled = true;
      }
    }
  }
}
