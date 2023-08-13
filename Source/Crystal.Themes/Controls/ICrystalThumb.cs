namespace Crystal.Themes.Controls;

public interface ICrystalThumb : IInputElement
{
  event DragStartedEventHandler DragStarted;

  event DragDeltaEventHandler DragDelta;

  event DragCompletedEventHandler DragCompleted;

  event MouseButtonEventHandler MouseDoubleClick;
}