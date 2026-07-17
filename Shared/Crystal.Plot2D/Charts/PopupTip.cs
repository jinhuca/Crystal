using System;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace Crystal.Plot2D.Charts;

public sealed class PopupTip : Popup {
  private readonly TimeSpan showDurationInterval = new(hours: 0, minutes: 0, seconds: 10);
  private Timer timer;

  public void ShowDelayed(TimeSpan delay) {
    if(timer != null) {
      timer.Change(dueTime: (int)delay.TotalMilliseconds, period: Timeout.Infinite);
    }
    else {
      timer = new Timer(callback: OnTimerFinished, state: null, dueTime: (int)delay.TotalMilliseconds, period: Timeout.Infinite);
    }
  }

  public void HideDelayed(TimeSpan delay) {
    if(timer != null) {
      timer.Change(dueTime: (int)delay.TotalMilliseconds, period: Timeout.Infinite);
    }
    else {
      timer = new Timer(callback: OnTimerFinished, state: null, dueTime: (int)delay.TotalMilliseconds, period: Timeout.Infinite);
    }
  }

  public void Hide() {
    if(timer != null) {
      timer.Change(dueTime: Timeout.Infinite, period: Timeout.Infinite);
    }
    IsOpen = false;
  }

  private void OnTimerFinished(object state) {
    Dispatcher.BeginInvoke(method: new Action(() => {
      var show_ = !IsOpen;
      IsOpen = show_;
      if(show_) {
        HideDelayed(delay: showDurationInterval);
      }
    }));
  }
}