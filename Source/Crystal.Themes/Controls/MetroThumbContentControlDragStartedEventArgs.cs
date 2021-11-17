namespace Crystal.Themes.Controls
{
  public class MetroThumbContentControlDragStartedEventArgs : DragStartedEventArgs
  {
    public MetroThumbContentControlDragStartedEventArgs(double horizontalOffset, double verticalOffset)
        : base(horizontalOffset, verticalOffset)
    {
      RoutedEvent = CrystalThumbContentControl.DragStartedEvent;
    }
  }
}