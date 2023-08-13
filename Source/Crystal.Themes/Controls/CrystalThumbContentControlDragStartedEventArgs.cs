namespace Crystal.Themes.Controls;

public class CrystalThumbContentControlDragStartedEventArgs : DragStartedEventArgs
{
  public CrystalThumbContentControlDragStartedEventArgs(double horizontalOffset, double verticalOffset)
    : base(horizontalOffset, verticalOffset)
  {
    RoutedEvent = CrystalThumbContentControl.DragStartedEvent;
  }
}