using Crystal.Plot2D.Common.Auxiliary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents a label provider for major ticks of <see cref="System.DateTime"/> type.
/// </summary>
public sealed class MajorDateTimeLabelProvider : DateTimeLabelProviderBase {
  /// <summary>
  /// Initializes a new instance of the <see cref="MajorDateTimeLabelProvider"/> class.
  /// </summary>
  public MajorDateTimeLabelProvider() { }

  public override UIElement[] CreateLabels(ITicksInfo<System.DateTime> ticksInfo) {
    var info = ticksInfo.Info;
    var ticks = ticksInfo.Ticks;
    var res = new UIElement[ticks.Length - 1];
    var labelsNum = 3;

    if(info is DifferenceIn differenceIn) {
      DateFormat = GetDateFormat(diff: differenceIn);
    }
    else if(info is MajorLabelsInfo majorLabelsInfo) {
      var diff = (DifferenceIn)majorLabelsInfo.Info;
      DateFormat = GetDateFormat(diff: diff);
      labelsNum = majorLabelsInfo.MajorLabelsCount + 1;

      //DebugVerify.Is(labelsNum < 100);
    }

    DebugVerify.Is(condition: ticks.Length < 10);

    LabelTickInfo<System.DateTime> tickInfo = new();
    for(var i = 0; i < ticks.Length - 1; i++) {
      tickInfo.Info = info;
      tickInfo.Tick = ticks[i];

      var tickText = GetString(tickInfo: tickInfo);

      Grid grid = new() {
        AllowDrop = false,
        CacheMode = null,
        Clip = null,
        ClipToBounds = false,
        Effect = null,
        Focusable = false,
        IsEnabled = false,
        IsHitTestVisible = false,
        IsManipulationEnabled = false,
        Opacity = 0,
        OpacityMask = null,
        RenderSize = default,
        RenderTransform = null,
        RenderTransformOrigin = default,
        SnapsToDevicePixels = false,
        Uid = null,
        Visibility = Visibility.Visible,
        BindingGroup = null,
        ContextMenu = null,
        Cursor = null,
        DataContext = null,
        FlowDirection = FlowDirection.LeftToRight,
        FocusVisualStyle = null,
        ForceCursor = false,
        Height = 0,
        HorizontalAlignment = HorizontalAlignment.Left,
        InputScope = null,
        Language = null,
        LayoutTransform = null,
        Margin = default,
        MaxHeight = 0,
        MaxWidth = 0,
        MinHeight = 0,
        MinWidth = 0,
        Name = null,
        OverridesDefaultStyle = false,
        Resources = null,
        Style = null,
        Tag = null,
        ToolTip = null,
        UseLayoutRounding = false,
        VerticalAlignment = VerticalAlignment.Top,
        Width = 0,
        Background = null,
        IsItemsHost = false,
        ShowGridLines = false
      };

      // doing binding as described at http://sdolha.spaces.live.com/blog/cns!4121802308C5AB4E!3724.entry?wa=wsignin1.0&sa=835372863

      grid.SetBinding(dp: Panel.BackgroundProperty, binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: DateTimeAxis.MajorLabelBackgroundBrushProperty), RelativeSource = new RelativeSource(mode: RelativeSourceMode.FindAncestor) { AncestorType = typeof(AxisControlBase) } });
      Rectangle rect = new() {
        StrokeThickness = 2
      };
      rect.SetBinding(dp: Shape.StrokeProperty, binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: DateTimeAxis.MajorLabelRectangleBorderPropertyProperty), RelativeSource = new RelativeSource(mode: RelativeSourceMode.FindAncestor) { AncestorType = typeof(AxisControlBase) } });

      Grid.SetColumn(element: rect, value: 0);
      Grid.SetColumnSpan(element: rect, value: labelsNum);

      for(var j = 0; j < labelsNum; j++) {
        grid.ColumnDefinitions.Add(value: new ColumnDefinition());
      }

      grid.Children.Add(element: rect);

      for(var j = 0; j < labelsNum; j++) {
        var tb = new TextBlock {
          Text = tickText,
          HorizontalAlignment = HorizontalAlignment.Center,
          Margin = new Thickness(left: 0, top: 3, right: 0, bottom: 3)
        };
        Grid.SetColumn(element: tb, value: j);
        grid.Children.Add(element: tb);
      }

      ApplyCustomView(info: tickInfo, label: grid);

      res[i] = grid;
    }

    return res;
  }

  protected override string GetDateFormat(DifferenceIn diff) {
    string format = null;

    switch(diff) {
      case DifferenceIn.Year:
        format = "yyyy";
        break;
      case DifferenceIn.Month:
        format = "MMMM yyyy";
        break;
      case DifferenceIn.Day:
        format = "%d MMMM yyyy";
        break;
      case DifferenceIn.Hour:
        format = "HH:mm %d MMMM yyyy";
        break;
      case DifferenceIn.Minute:
        format = "HH:mm %d MMMM yyyy";
        break;
      case DifferenceIn.Second:
        format = "HH:mm:ss %d MMMM yyyy";
        break;
      case DifferenceIn.Millisecond:
        format = "fff";
        break;
    }

    return format;
  }
}
