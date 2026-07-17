using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents default implementation of label provider for specified type.
/// </summary>
/// <typeparam name="T">Axis values type.</typeparam>
public sealed class GenericLabelProvider<T> : LabelProviderBase<T> {
  /// <summary>
  /// Initializes a new instance of the <see cref="GenericLabelProvider&lt;T&gt;"/> class.
  /// </summary>
  public GenericLabelProvider() { }

  #region ILabelProvider<T> Members

  /// <summary>
  /// Creates the labels by given ticks info.
  /// </summary>
  /// <param name="ticksInfo">The ticks info.</param>
  /// <returns>
  /// Array of <see cref="UIElement"/>s, which are axis labels for specified axis ticks.
  /// </returns>
  public override UIElement[] CreateLabels(ITicksInfo<T> ticksInfo) {
    var ticks = ticksInfo.Ticks;
    var info = ticksInfo.Info;

    LabelTickInfo<T> tickInfo = new();
    var res = new UIElement[ticks.Length];
    for(var i = 0; i < res.Length; i++) {
      tickInfo.Tick = ticks[i];
      tickInfo.Info = info;

      var text = GetString(tickInfo: tickInfo);

      res[i] = new TextBlock {
        Text = text,
        ToolTip = ticks[i].ToString()
      };
    }
    return res;
  }

  #endregion
}
