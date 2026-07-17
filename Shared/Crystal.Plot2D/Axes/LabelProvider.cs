using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes;

public abstract class LabelProvider<T> : LabelProviderBase<T> {
  public override UIElement[] CreateLabels(ITicksInfo<T> ticksInfo) {
    var ticks = ticksInfo.Ticks;

    var res = new UIElement[ticks.Length];
    LabelTickInfo<T> labelInfo = new() { Info = ticksInfo.Info };

    for(var i = 0; i < res.Length; i++) {
      labelInfo.Tick = ticks[i];
      labelInfo.Index = i;

      var labelText = GetString(tickInfo: labelInfo);

      var label = (TextBlock)GetResourceFromPool() ?? new TextBlock();

      label.Text = labelText;
      label.ToolTip = ticks[i].ToString();

      res[i] = label;

      ApplyCustomView(info: labelInfo, label: label);
    }

    return res;
  }
}
