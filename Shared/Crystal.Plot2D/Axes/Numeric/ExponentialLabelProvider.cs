using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Crystal.Plot2D.Axes.Numeric;

/// <summary>
/// Represents an axis label provider for double ticks, generating labels with numbers in exponential form when it is appropriate.
/// </summary>
public sealed class ExponentialLabelProvider : NumericLabelProviderBase {
  /// <summary>
  /// Initializes a new instance of the <see cref="ExponentialLabelProvider"/> class.
  /// </summary>
  public ExponentialLabelProvider() { }

  /// <summary>
  /// Creates labels by given ticks info.
  /// Is not intended to be called from your code.
  /// </summary>
  /// <param name="ticksInfo">The ticks info.</param>
  /// <returns>
  /// Array of <see cref="UIElement"/>s, which are axis labels for specified axis ticks.
  /// </returns>
  public override UIElement[] CreateLabels(ITicksInfo<double> ticksInfo) {
    var ticks = ticksInfo.Ticks;

    Init(ticks: ticks);

    var res = new UIElement[ticks.Length];

    LabelTickInfo<double> tickInfo = new() { Info = ticksInfo.Info };

    for(var i = 0; i < res.Length; i++) {
      var tick = ticks[i];
      tickInfo.Tick = tick;
      tickInfo.Index = i;

      var labelText = GetString(tickInfo: tickInfo);

      TextBlock label;
      if(labelText.Contains(value: 'E')) {
        var substrs = labelText.Split(separator: 'E');
        var mantissa = substrs[0];
        var exponenta = substrs[1];
        exponenta = exponenta.TrimStart(trimChar: '+');
        Span span = new();
        span.Inlines.Add(text: string.Format(provider: CultureInfo.CurrentCulture, format: "{0}·10", arg0: mantissa));
        Span exponentaSpan = new(childInline: new Run(text: exponenta));
        exponentaSpan.BaselineAlignment = BaselineAlignment.Superscript;
        exponentaSpan.FontSize = 8;
        span.Inlines.Add(item: exponentaSpan);

        label = new TextBlock(inline: span);
        LabelProviderProperties.SetExponentialIsCommonLabel(obj: label, value: false);
      }
      else {
        label = (TextBlock)GetResourceFromPool() ?? new TextBlock();

        label.Text = labelText;
      }

      res[i] = label;
      label.ToolTip = tick.ToString(provider: CultureInfo.CurrentCulture);

      ApplyCustomView(info: tickInfo, label: label);
    }

    return res;
  }

  protected override bool ReleaseCore(UIElement label) {
    var isNotExponential = LabelProviderProperties.GetExponentialIsCommonLabel(obj: label);
    return isNotExponential && CustomView == null;
  }
}
