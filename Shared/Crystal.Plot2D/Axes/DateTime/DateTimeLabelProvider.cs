using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents a label provider for <see cref="System.DateTime"/> ticks.
/// </summary>
public sealed class DateTimeLabelProvider : DateTimeLabelProviderBase {
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeLabelProvider"/> class.
  /// </summary>
  public DateTimeLabelProvider() { }

  public override UIElement[] CreateLabels(ITicksInfo<System.DateTime> ticksInfo) {
    var info = ticksInfo.Info;
    var ticks = ticksInfo.Ticks;

    if(info is DifferenceIn diff) {
      DateFormat = GetDateFormat(diff: diff);
    }

    LabelTickInfo<System.DateTime> tickInfo = new() { Info = info };

    var res = new UIElement[ticks.Length];
    for(var i = 0; i < ticks.Length; i++) {
      tickInfo.Tick = ticks[i];

      var tickText = GetString(tickInfo: tickInfo);
      UIElement label = new TextBlock { Text = tickText, ToolTip = ticks[i] };
      ApplyCustomView(info: tickInfo, label: label);
      res[i] = label;
    }

    return res;
  }
}
