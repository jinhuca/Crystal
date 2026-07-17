using System;

namespace Crystal.Plot2D.Common.NotifyingPanels;

internal interface INotifyingPanel {
  NotifyingUIElementCollection NotifyingChildren { get; }
  event EventHandler ChildrenCreated;
}
