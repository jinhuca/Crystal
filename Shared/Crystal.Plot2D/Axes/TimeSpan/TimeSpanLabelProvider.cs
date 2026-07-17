using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes.TimeSpan;

public sealed class TimeSpanLabelProvider : LabelProviderBase<System.TimeSpan> {
  public override UIElement[] CreateLabels(ITicksInfo<System.TimeSpan> ticksInfo) {
    var info_ = ticksInfo.Info;
    var ticks_ = ticksInfo.Ticks;

    LabelTickInfo<System.TimeSpan> tickInfo_ = new();

    var res_ = new UIElement[ticks_.Length];
    for(var i_ = 0; i_ < ticks_.Length; i_++) {
      tickInfo_.Tick = ticks_[i_];
      tickInfo_.Info = info_;

      var tickText_ = GetString(tickInfo: tickInfo_);
      UIElement label_ = new TextBlock { Text = tickText_, ToolTip = ticks_[i_] };
      res_[i_] = label_;
    }

    return res_;
  }
}
