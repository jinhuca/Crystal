namespace Crystal.Themes.Controls
{
  public class CrystalThumbContentControlDragCompletedEventArgs : DragCompletedEventArgs
  {
    public CrystalThumbContentControlDragCompletedEventArgs(double horizontalOffset, double verticalOffset, bool canceled)
        : base(horizontalOffset, verticalOffset, canceled)
    {
      RoutedEvent = CrystalThumbContentControl.DragCompletedEvent;
    }
  }
}