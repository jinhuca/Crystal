using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Defines a base class for axis UI representation.
/// Contains a number of properties that can be used to adjust ticks set and their look.
/// </summary>
/// <typeparam name="T"></typeparam>
[TemplatePart(Name = "PART_AdditionalLabelsCanvas", Type = typeof(StackCanvas))]
[TemplatePart(Name = "PART_CommonLabelsCanvas", Type = typeof(StackCanvas))]
[TemplatePart(Name = "PART_TicksPath", Type = typeof(Path))]
[TemplatePart(Name = "PART_ContentsGrid", Type = typeof(Grid))]
public abstract class AxisControl<T> : AxisControlBase {
  private const string TemplateKey = "axisControlTemplate";
  private const string AdditionalLabelTransformKey = "additionalLabelsTransform";
  private const string PartAdditionalLabelsCanvas = "PART_AdditionalLabelsCanvas";
  private const string PartCommonLabelsCanvas = "PART_CommonLabelsCanvas";
  private const string PartTicksPath = "PART_TicksPath";
  private const string PartContentsGrid = "PART_ContentsGrid";

  /// <summary>
  /// Initializes a new instance of the <see cref="AxisControl&lt;T&gt;"/> class.
  /// </summary>
  protected AxisControl() {
    HorizontalContentAlignment = HorizontalAlignment.Stretch;
    VerticalContentAlignment = VerticalAlignment.Stretch;

    Background = Brushes.Transparent;
    //ClipToBounds = true;
    Focusable = false;

    UpdateUIResources();
    UpdateSizeGetters();
  }

  internal void MakeDependent() => independent = false;

  /// <summary>
  /// This conversion is performed to make horizontal one-string and two-string labels
  /// stay at one height.
  /// </summary>
  /// <param name="placement"></param>
  /// <returns></returns>
  private static AxisPlacement GetBetterPlacement(AxisPlacement placement) {
    return placement switch {
      AxisPlacement.Left => AxisPlacement.Left,
      AxisPlacement.Right => AxisPlacement.Right,
      AxisPlacement.Top => AxisPlacement.Top,
      AxisPlacement.Bottom => AxisPlacement.Bottom,
      _ => throw new NotSupportedException()
    };
  }

  #region Properties

  private AxisPlacement placement = AxisPlacement.Bottom;

  /// <summary>
  /// Gets or sets the placement of axis control.
  /// Relative positioning of parts of axis depends on this value.
  /// </summary>
  /// <value>The placement.</value>
  public AxisPlacement Placement {
    get => placement;
    set {
      if(placement != value) {
        placement = value;
        UpdateUIResources();
        UpdateSizeGetters();
      }
    }
  }

  private void UpdateSizeGetters() {
    switch(placement) {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        getSize = size => size.Height;
        getCoordinate = p => p.Y;
        createScreenPoint1 = d => new Point(x: ScrCoord1, y: d);
        createScreenPoint2 = (d, size) => new Point(x: scrCoord2 * size, y: d);
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        getSize = size => size.Width;
        getCoordinate = p => p.X;
        createScreenPoint1 = d => new Point(x: d, y: ScrCoord1);
        createScreenPoint2 = (d, size) => new Point(x: d, y: scrCoord2 * size);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    createDataPoint = placement switch {
      AxisPlacement.Left => d => new Point(x: 0, y: d),
      AxisPlacement.Right => d => new Point(x: 1, y: d),
      AxisPlacement.Top => d => new Point(x: d, y: 1),
      AxisPlacement.Bottom => d => new Point(x: d, y: 0),
      _ => createDataPoint
    };
  }

  private void UpdateUIResources() {
    ResourceDictionary resources_ = new() {
      Source = new Uri(uriString: Constants.Constants.AxisResourceUri, uriKind: UriKind.Relative)
    };

    var placement_ = GetBetterPlacement(placement: placement);
    var template_ = (ControlTemplate)resources_[key: TemplateKey + placement_];
    Verify.AssertNotNull(obj: template_);
    var content_ = (FrameworkElement)template_.LoadContent();

    if(_ticksPath != null && _ticksPath.Data != null) {
      var group_ = (GeometryGroup)_ticksPath.Data;
      foreach(var child_ in group_.Children) {
        var geometry_ = (LineGeometry)child_;
        lineGeomPool.Put(item: geometry_);
      }
      group_.Children.Clear();
    }

    _ticksPath = (Path)content_.FindName(name: PartTicksPath);
    _ticksPath.SnapsToDevicePixels = true;
    Verify.AssertNotNull(obj: _ticksPath);

    // as this method can be called not only on loading of axisControl, but when its placement changes, internal panels
    // can be not empty and their contents should be released
    if(commonLabelsCanvas != null && labelProvider != null) {
      foreach(UIElement child_ in commonLabelsCanvas.Children) {
        if(child_ != null) {
          labelProvider.ReleaseLabel(label: child_);
        }
      }

      labels = null;
      commonLabelsCanvas.Children.Clear();
    }

    commonLabelsCanvas = (StackCanvas)content_.FindName(name: PartCommonLabelsCanvas);
    Verify.AssertNotNull(obj: commonLabelsCanvas);
    commonLabelsCanvas.Placement = placement_;

    if(additionalLabelsCanvas != null && majorLabelProvider != null) {
      foreach(UIElement child_ in additionalLabelsCanvas.Children) {
        if(child_ != null) {
          majorLabelProvider.ReleaseLabel(label: child_);
        }
      }
    }

    additionalLabelsCanvas = (StackCanvas)content_.FindName(name: PartAdditionalLabelsCanvas);
    Verify.AssertNotNull(obj: additionalLabelsCanvas);
    additionalLabelsCanvas.Placement = placement_;

    mainGrid = (Grid)content_.FindName(name: PartContentsGrid);
    Verify.AssertNotNull(obj: mainGrid);

    mainGrid.SetBinding(dp: BackgroundProperty, binding: new Binding { Path = new PropertyPath(path: "Background"), Source = this });
    mainGrid.SizeChanged += mainGrid_SizeChanged;

    Content = mainGrid;

    var transformKey_ = AdditionalLabelTransformKey + placement_;
    if(resources_.Contains(key: transformKey_)) {
      additionalLabelTransform = (Transform)resources_[key: transformKey_];
    }
  }

  private void mainGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
    if(placement.IsBottomOrTop() && e.WidthChanged ||
       e.HeightChanged) {
      // this is performed because if not, whole axisControl's size was measured wrongly.
      InvalidateMeasure();
      UpdateUI();
    }
  }

  private bool updateOnCommonChange = true;

  internal IDisposable OpenUpdateRegion(bool forceUpdate) {
    return new UpdateRegionHolder<T>(owner: this, forceUpdate: forceUpdate);
  }

  private sealed class UpdateRegionHolder<TT> : IDisposable {
    private readonly Range<TT> prevRange;
    private readonly CoordinateTransform prevTransform;
    private AxisControl<TT> owner;
    private readonly bool forceUpdate;

    public UpdateRegionHolder(AxisControl<TT> owner) : this(owner: owner, forceUpdate: false) { }

    public UpdateRegionHolder(AxisControl<TT> owner, bool forceUpdate) {
      this.owner = owner;
      owner.updateOnCommonChange = false;

      prevTransform = owner.transform;
      prevRange = owner.range;
      this.forceUpdate = forceUpdate;
    }

    #region IDisposable Members

    public void Dispose() {
      owner.updateOnCommonChange = true;

      var shouldUpdate_ = owner.range != prevRange;

      var screenRect_ = owner.Transform.ScreenRect;
      var prevScreenRect_ = prevTransform.ScreenRect;
      if(owner.placement.IsBottomOrTop()) {
        shouldUpdate_ |= prevScreenRect_.Width != screenRect_.Width;
      }
      else {
        shouldUpdate_ |= prevScreenRect_.Height != screenRect_.Height;
      }

      shouldUpdate_ |= owner.transform.DataTransform != prevTransform.DataTransform;
      shouldUpdate_ |= forceUpdate;

      if(shouldUpdate_) {
        owner.UpdateUI();
      }
      owner = null;
    }

    #endregion
  }

  private Range<T> range;
  /// <summary>
  /// Gets or sets the range, which ticks are generated for.
  /// </summary>
  /// <value>The range.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Range<T> Range {
    get => range;
    set {
      range = value;
      if(updateOnCommonChange) {
        UpdateUI();
      }
    }
  }

  private bool drawMinorTicks = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show minor ticks.
  /// </summary>
  /// <value><c>true</c> if show minor ticks; otherwise, <c>false</c>.</value>
  public bool DrawMinorTicks {
    get => drawMinorTicks;
    set {
      if(drawMinorTicks != value) {
        drawMinorTicks = value;
        UpdateUI();
      }
    }
  }

  private bool drawMajorLabels = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show major labels.
  /// </summary>
  /// <value><c>true</c> if show major labels; otherwise, <c>false</c>.</value>
  public bool DrawMajorLabels {
    get => drawMajorLabels;
    set {
      if(drawMajorLabels != value) {
        drawMajorLabels = value;
        UpdateUI();
      }
    }
  }

  private bool drawTicks = true;
  public bool DrawTicks {
    get => drawTicks;
    set {
      if(drawTicks != value) {
        drawTicks = value;
        UpdateUI();
      }
    }
  }

  #region TicksProvider

  private ITicksProvider<T> ticksProvider;
  /// <summary>
  /// Gets or sets the ticks provider - generator of ticks for given range.
  /// 
  /// Should not be null.
  /// </summary>
  /// <value>The ticks provider.</value>
  public ITicksProvider<T> TicksProvider {
    get => ticksProvider;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(ticksProvider != value) {
        DetachTicksProvider();

        ticksProvider = value;

        AttachTicksProvider();

        UpdateUI();
      }
    }
  }

  private void AttachTicksProvider() {
    if(ticksProvider != null) {
      ticksProvider.Changed += ticksProvider_Changed;
    }
  }

  private void ticksProvider_Changed(object sender, EventArgs e) {
    UpdateUI();
  }

  private void DetachTicksProvider() {
    if(ticksProvider != null) {
      ticksProvider.Changed -= ticksProvider_Changed;
    }
  }

  #endregion

  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override bool ShouldSerializeContent() {
    return false;
  }

  protected override bool ShouldSerializeProperty(DependencyProperty dp) {
    // do not serialize template - for XAML serialization
    if(dp == TemplateProperty) {
      return false;
    }

    return base.ShouldSerializeProperty(dp: dp);
  }

  #region MajorLabelProvider

  private LabelProviderBase<T> majorLabelProvider;
  /// <summary>
  /// Gets or sets the major label provider, which creates labels for major ticks.
  /// If null, major labels will not be shown.
  /// </summary>
  /// <value>The major label provider.</value>
  public LabelProviderBase<T> MajorLabelProvider {
    get => majorLabelProvider;
    set {
      if(majorLabelProvider != value) {
        DetachMajorLabelProvider();

        majorLabelProvider = value;

        AttachMajorLabelProvider();

        UpdateUI();
      }
    }
  }

  private void AttachMajorLabelProvider() {
    if(majorLabelProvider != null) {
      majorLabelProvider.Changed += majorLabelProvider_Changed;
    }
  }

  private void majorLabelProvider_Changed(object sender, EventArgs e) {
    UpdateUI();
  }

  private void DetachMajorLabelProvider() {
    if(majorLabelProvider != null) {
      majorLabelProvider.Changed -= majorLabelProvider_Changed;
    }
  }

  #endregion

  #region LabelProvider

  private LabelProviderBase<T> labelProvider;
  /// <summary>
  /// Gets or sets the label provider, which generates labels for axis ticks.
  /// Should not be null.
  /// </summary>
  /// <value>The label provider.</value>
  [NotNull]
  public LabelProviderBase<T> LabelProvider {
    get => labelProvider;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(labelProvider != value) {
        DetachLabelProvider();

        labelProvider = value;

        AttachLabelProvider();

        UpdateUI();
      }
    }
  }

  private void AttachLabelProvider() {
    if(labelProvider != null) {
      labelProvider.Changed += labelProvider_Changed;
    }
  }

  private void labelProvider_Changed(object sender, EventArgs e) {
    UpdateUI();
  }

  private void DetachLabelProvider() {
    if(labelProvider != null) {
      labelProvider.Changed -= labelProvider_Changed;
    }
  }

  #endregion

  private CoordinateTransform transform = CoordinateTransform.CreateDefault();
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public CoordinateTransform Transform {
    get => transform;
    set {
      transform = value;
      if(updateOnCommonChange) {
        UpdateUI();
      }
    }
  }

  #endregion

  private const double DefaultSmallerSize = 1;
  private const double DefaultBiggerSize = 150;
  protected override Size MeasureOverride(Size constraint) {
    var baseSize_ = base.MeasureOverride(constraint: constraint);

    mainGrid.Measure(availableSize: constraint);
    var gridSize_ = mainGrid.DesiredSize;
    var result_ = gridSize_;

    var isHorizontal_ = placement == AxisPlacement.Bottom || placement == AxisPlacement.Top;
    if(double.IsInfinity(d: constraint.Width) && isHorizontal_) {
      result_ = new Size(width: DefaultBiggerSize, height: gridSize_.Height != 0 ? gridSize_.Height : DefaultSmallerSize);
    }
    else if(double.IsInfinity(d: constraint.Height) && !isHorizontal_) {
      result_ = new Size(width: gridSize_.Width != 0 ? gridSize_.Width : DefaultSmallerSize, height: DefaultBiggerSize);
    }

    return result_;
  }

  //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
  //{
  //    base.OnRenderSizeChanged(sizeInfo);

  //    bool isHorizontal = placement == AxisPlacement.Top || placement == AxisPlacement.Bottom;
  //    if (isHorizontal && sizeInfo.WidthChanged || !isHorizontal && sizeInfo.HeightChanged)
  //    {
  //        UpdateUIRepresentation();
  //    }
  //}

  private void InitTransform(Size newRenderSize) {
    var dataRect_ = CreateDataRect();

    transform = transform.WithRects(visibleRect: dataRect_, screenRect: new Rect(size: newRenderSize));
  }

  private Rect CreateDataRect() {
    var min_ = convertToDouble(arg: range.Min);
    var max_ = convertToDouble(arg: range.Max);

    Rect dataRect_;
    switch(placement) {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        dataRect_ = new Rect(point1: new Point(x: min_, y: min_), point2: new Point(x: max_, y: max_));
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        dataRect_ = new Rect(point1: new Point(x: min_, y: min_), point2: new Point(x: max_, y: max_));
        break;
      default:
        throw new NotSupportedException();
    }
    return dataRect_;
  }

  private Path _ticksPath;

  public override Path TicksPath {
    get => _ticksPath;
    set => _ticksPath = value;
  }

  private Grid mainGrid;
  private StackCanvas additionalLabelsCanvas;
  private StackCanvas commonLabelsCanvas;

  private bool rendered;

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(drawingContext: dc);

    if(!rendered) {
      UpdateUI();
    }
    rendered = true;
  }

  private bool independent = true;

  private const double ScrCoord1 = 0; // px
  private double scrCoord2 = 10; // px

  /// <summary>
  /// Gets or sets the size of main axis ticks.
  /// </summary>
  /// <value>The size of the tick.</value>
  public double TickSize {
    get => scrCoord2;
    set {
      if(scrCoord2 != value) {
        scrCoord2 = value;
        UpdateUI();
      }
    }
  }

  private GeometryGroup geomGroup = new();
  internal void UpdateUI() {
    if(range.IsEmpty) {
      return;
    }

    if(transform == null) {
      return;
    }

    if(independent) {
      InitTransform(newRenderSize: RenderSize);
    }

    var isHorizontal_ = Placement == AxisPlacement.Bottom || Placement == AxisPlacement.Top;
    if(transform.ScreenRect.Width == 0 && isHorizontal_
        || transform.ScreenRect.Height == 0 && !isHorizontal_) {
      return;
    }

    if(!IsMeasureValid) {
      InvalidateMeasure();
    }

    CreateTicks();

    // removing infinite screen ticks
    var tempTicks_ = new List<T>(collection: ticks);
    var tempScreenTicks_ = new List<double>(capacity: ticks.Length);
    var tempLabels_ = new List<UIElement>(collection: labels);

    var i_ = 0;
    while(i_ < tempTicks_.Count) {
      var tick_ = tempTicks_[index: i_];
      var screenTick_ = getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: tick_)).DataToScreen(transform: transform));
      if(screenTick_.IsFinite()) {
        tempScreenTicks_.Add(item: screenTick_);
        i_++;
      }
      else {
        tempTicks_.RemoveAt(index: i_);
        tempLabels_.RemoveAt(index: i_);
      }
    }

    ticks = tempTicks_.ToArray();
    screenTicks = tempScreenTicks_.ToArray();
    labels = tempLabels_.ToArray();

    // saving generated lines into pool
    for(i_ = 0; i_ < geomGroup.Children.Count; i_++) {
      var geometry_ = (LineGeometry)geomGroup.Children[index: i_];
      lineGeomPool.Put(item: geometry_);
    }

    geomGroup = new GeometryGroup {
      Children = new GeometryCollection(capacity: lineGeomPool.Count)
    };

    if(drawTicks) {
      DoDrawTicks(screenTicksX: screenTicks, lines: geomGroup.Children);
    }

    if(drawMinorTicks) {
      DoDrawMinorTicks(lines: geomGroup.Children);
    }

    _ticksPath.Data = geomGroup;

    DoDrawCommonLabels(screenTicksX: screenTicks);

    if(drawMajorLabels) {
      DoDrawMajorLabels();
    }

    ScreenTicksChanged.Raise(sender: this);
  }

  private bool drawTicksOnEmptyLabel;
  /// <summary>
  /// Gets or sets a value indicating whether to draw ticks on empty label.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if draw ticks on empty label; otherwise, <c>false</c>.
  /// </value>
  public bool DrawTicksOnEmptyLabel {
    get => drawTicksOnEmptyLabel;
    set {
      if(drawTicksOnEmptyLabel != value) {
        drawTicksOnEmptyLabel = value;
        UpdateUI();
      }
    }
  }

  private readonly ResourcePool<LineGeometry> lineGeomPool = new();
  private void DoDrawTicks(double[] screenTicksX, ICollection<Geometry> lines) {
    for(var i_ = 0; i_ < screenTicksX.Length; i_++) {
      if(labels[i_] == null && !drawTicksOnEmptyLabel) {
        continue;
      }

      var p1_ = createScreenPoint1(arg: screenTicksX[i_]);
      var p2_ = createScreenPoint2(arg1: screenTicksX[i_], arg2: 1);

      var line_ = lineGeomPool.GetOrCreate();

      line_.StartPoint = p1_;
      line_.EndPoint = p2_;
      lines.Add(item: line_);
    }
  }

  private double GetRangesRatio(Range<T> nominator, Range<T> denominator) {
    var nomMin_ = ConvertToDouble(arg: nominator.Min);
    var nomMax_ = ConvertToDouble(arg: nominator.Max);
    var denMin_ = ConvertToDouble(arg: denominator.Min);
    var denMax_ = ConvertToDouble(arg: denominator.Max);

    return (nomMax_ - nomMin_) / (denMax_ - denMin_);
  }

  private Transform additionalLabelTransform;
  private void DoDrawMajorLabels() {
    var majorTicksProvider_ = ticksProvider.MajorProvider;
    additionalLabelsCanvas.Children.Clear();

    if(majorTicksProvider_ != null && majorLabelProvider != null) {
      additionalLabelsCanvas.Visibility = Visibility.Visible;

      var renderSize_ = RenderSize;
      var majorTicks_ = majorTicksProvider_.GetTicks(range: range, ticksCount: DefaultTicksProvider.DefaultTicksCount);

      var screenCoords_ = majorTicks_.Ticks.Select(selector: tick => createDataPoint(arg: convertToDouble(arg: tick))).
          Select(selector: p => p.DataToScreen(transform: transform)).Select(selector: p => getCoordinate(arg: p)).ToArray();

      // todo this is not the best decision - when displaying, for example,
      // milliseconds, it causes to create hundreds and thousands of textBlocks.
      var rangesRatio_ = GetRangesRatio(nominator: majorTicks_.Ticks.GetPairs().ToArray()[0], denominator: range);

      var info_ = majorTicks_.Info;
      MajorLabelsInfo newInfo_ = new() {
        Info = info_,
        MajorLabelsCount = (int)Math.Ceiling(a: rangesRatio_)
      };

      var newMajorTicks_ = new TicksInfo<T> {
        Info = newInfo_,
        Ticks = majorTicks_.Ticks,
        TickSizes = majorTicks_.TickSizes
      };

      var additionalLabels_ = MajorLabelProvider.CreateLabels(ticksInfo: newMajorTicks_);

      for(var i_ = 0; i_ < additionalLabels_.Length; i_++) {
        if(screenCoords_[i_].IsNaN()) {
          continue;
        }

        var tickLabel_ = additionalLabels_[i_];

        tickLabel_.Measure(availableSize: renderSize_);

        StackCanvas.SetCoordinate(obj: tickLabel_, value: screenCoords_[i_]);
        StackCanvas.SetEndCoordinate(obj: tickLabel_, value: screenCoords_[i_ + 1]);

        if(tickLabel_ is FrameworkElement) {
          ((FrameworkElement)tickLabel_).LayoutTransform = additionalLabelTransform;
        }

        additionalLabelsCanvas.Children.Add(element: tickLabel_);
      }
    }
    else {
      additionalLabelsCanvas.Visibility = Visibility.Collapsed;
    }
  }

  private int prevMinorTicksCount = DefaultTicksProvider.DefaultTicksCount;
  private const int MaxTickArrangeIterations = 12;
  private void DoDrawMinorTicks(ICollection<Geometry> lines) {
    var minorTicksProvider_ = ticksProvider.MinorProvider;
    if(minorTicksProvider_ != null) {
      var minorTicksCount_ = prevMinorTicksCount;
      ITicksInfo<T> minorTicks_;
      var result_ = TickCountChange.Ok;
      var iteration_ = 0;
      do {
        Verify.IsTrue(condition: ++iteration_ < MaxTickArrangeIterations);

        minorTicks_ = minorTicksProvider_.GetTicks(range: range, ticksCount: minorTicksCount_);

        var prevResult_ = result_;
        result_ = CheckMinorTicksArrangement(minorTicks: minorTicks_);
        if(prevResult_ == TickCountChange.Decrease && result_ == TickCountChange.Increase) {
          // stop tick number oscillating
          result_ = TickCountChange.Ok;
        }

        if(result_ == TickCountChange.Decrease) {
          var newMinorTicksCount_ = minorTicksProvider_.DecreaseTickCount(ticksCount: minorTicksCount_);
          if(newMinorTicksCount_ == minorTicksCount_) {
            result_ = TickCountChange.Ok;
          }

          minorTicksCount_ = newMinorTicksCount_;
        }
        else if(result_ == TickCountChange.Increase) {
          var newCount_ = minorTicksProvider_.IncreaseTickCount(ticksCount: minorTicksCount_);
          if(newCount_ == minorTicksCount_) {
            result_ = TickCountChange.Ok;
          }

          minorTicksCount_ = newCount_;
        }
      } while(result_ != TickCountChange.Ok);

      prevMinorTicksCount = minorTicksCount_;

      var sizes_ = minorTicks_.TickSizes;

      var screenCoords_ = minorTicks_.Ticks.Select(
        selector: coord => getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: coord)).DataToScreen(transform: transform))).ToArray();

      minorScreenTicks = new MinorTickInfo<double>[screenCoords_.Length];
      for(var i_ = 0; i_ < screenCoords_.Length; i_++) {
        minorScreenTicks[i_] = new MinorTickInfo<double>(value: sizes_[i_], tick: screenCoords_[i_]);
      }

      for(var i_ = 0; i_ < screenCoords_.Length; i_++) {
        var screenCoord_ = screenCoords_[i_];

        var p1_ = createScreenPoint1(arg: screenCoord_);
        var p2_ = createScreenPoint2(arg1: screenCoord_, arg2: sizes_[i_]);

        var line_ = lineGeomPool.GetOrCreate();
        line_.StartPoint = p1_;
        line_.EndPoint = p2_;

        lines.Add(item: line_);
      }
    }
  }

  private TickCountChange CheckMinorTicksArrangement(ITicksInfo<T> minorTicks) {
    var renderSize_ = RenderSize;
    var result_ = TickCountChange.Ok;
    if(minorTicks.Ticks.Length * 3 > getSize(arg: renderSize_)) {
      result_ = TickCountChange.Decrease;
    }
    else if(minorTicks.Ticks.Length * 6 < getSize(arg: renderSize_)) {
      result_ = TickCountChange.Increase;
    }

    return result_;
  }

  private bool isStaticAxis;
  /// <summary>
  /// Gets or sets a value indicating whether this instance is a static axis.
  /// If axis is static, its labels from sides are shifted so that they are not clipped by axis bounds.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if this instance is static axis; otherwise, <c>false</c>.
  /// </value>
  public bool IsStaticAxis {
    get => isStaticAxis;
    set {
      if(isStaticAxis != value) {
        isStaticAxis = value;
        UpdateUI();
      }
    }
  }

  private double ToScreen(T value) {
    return getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: value)).DataToScreen(transform: transform));
  }

  private const double StaticAxisMargin = 1; // px

  private void DoDrawCommonLabels(double[] screenTicksX) {
    var renderSize_ = RenderSize;

    commonLabelsCanvas.Children.Clear();

#if DEBUG
    if(labels != null) {
      foreach(FrameworkElement item_ in labels) {
        if(item_ != null) {
          Debug.Assert(condition: item_.Parent == null);
        }
      }
    }
#endif

    var minCoordUnsorted_ = ToScreen(value: range.Min);
    var maxCoordUnsorted_ = ToScreen(value: range.Max);

    var minCoord_ = Math.Min(val1: minCoordUnsorted_, val2: maxCoordUnsorted_);
    var maxCoord_ = Math.Max(val1: minCoordUnsorted_, val2: maxCoordUnsorted_);

    var maxCoordDiff_ = (maxCoord_ - minCoord_) / labels.Length / 2.0;

    var minCoordToAdd_ = minCoord_ - maxCoordDiff_;
    var maxCoordToAdd_ = maxCoord_ + maxCoordDiff_;

    for(var i_ = 0; i_ < ticks.Length; i_++) {
      var tickLabel_ = (FrameworkElement)labels[i_];
      if(tickLabel_ == null) {
        continue;
      }

      Debug.Assert(condition: tickLabel_.Parent == null);

      tickLabel_.Measure(availableSize: new Size(width: double.PositiveInfinity, height: double.PositiveInfinity));

      var screenX_ = screenTicksX[i_];
      var coord_ = screenX_;

      tickLabel_.HorizontalAlignment = HorizontalAlignment.Center;
      tickLabel_.VerticalAlignment = VerticalAlignment.Center;

      if(isStaticAxis) {
        // getting real size of label
        tickLabel_.Measure(availableSize: renderSize_);
        var tickLabelSize_ = tickLabel_.DesiredSize;

        if(Math.Abs(value: screenX_ - minCoord_) < maxCoordDiff_) {
          coord_ = minCoord_ + StaticAxisMargin;
          if(placement.IsBottomOrTop()) {
            tickLabel_.HorizontalAlignment = HorizontalAlignment.Left;
          }
          else {
            tickLabel_.VerticalAlignment = VerticalAlignment.Top;
          }
        }
        else if(Math.Abs(value: screenX_ - maxCoord_) < maxCoordDiff_) {
          coord_ = maxCoord_ - getSize(arg: tickLabelSize_) / 2 - StaticAxisMargin;
          if(!placement.IsBottomOrTop()) {
            tickLabel_.VerticalAlignment = VerticalAlignment.Bottom;
            coord_ = maxCoord_ - StaticAxisMargin;
          }
        }
      }

      // label is out of visible area
      if(coord_ < minCoord_ || coord_ > maxCoord_) {
        continue;
      }

      if(coord_.IsNaN()) {
        continue;
      }

      StackCanvas.SetCoordinate(obj: tickLabel_, value: coord_);

      commonLabelsCanvas.Children.Add(element: tickLabel_);
    }
  }

  private double GetCoordinateFromTick(T tick) {
    return getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: tick)).DataToScreen(transform: transform));
  }

  private Func<T, double> convertToDouble;

  /// <summary>
  /// Gets or sets the conversation of tick to double.
  /// Should not be null.
  /// </summary>
  /// <value>The convert to double.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Func<T, double> ConvertToDouble {
    get => convertToDouble;
    set {
      ArgumentNullException.ThrowIfNull(value);

      convertToDouble = value;
      UpdateUI();
    }
  }

  internal event EventHandler ScreenTicksChanged;
  private double[] screenTicks;
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public double[] ScreenTicks => screenTicks;

  private MinorTickInfo<double>[] minorScreenTicks;
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public MinorTickInfo<double>[] MinorScreenTicks => minorScreenTicks;

  private ITicksInfo<T> ticksInfo;
  private T[] ticks;
  private UIElement[] labels;
  private const double IncreaseRatio = 3.0;
  private const double DecreaseRatio = 1.6;

  private Func<Size, double> getSize = size => size.Width;
  private Func<Point, double> getCoordinate = p => p.X;
  private Func<double, Point> createDataPoint = d => new Point(x: d, y: 0);

  private Func<double, Point> createScreenPoint1 = d => new Point(x: d, y: 0);
  private Func<double, double, Point> createScreenPoint2 = (d, size) => new Point(x: d, y: size);

  private int previousTickCount = DefaultTicksProvider.DefaultTicksCount;
  private void CreateTicks() {
    var result_ = TickCountChange.Ok;

    var prevActualTickCount_ = -1;

    var tickCount_ = previousTickCount;
    var iteration_ = 0;

    do {
      Verify.IsTrue(condition: ++iteration_ < MaxTickArrangeIterations);

      ticksInfo = ticksProvider.GetTicks(range: range, ticksCount: tickCount_);
      ticks = ticksInfo.Ticks;

      if(ticks.Length == prevActualTickCount_) {
        break;
      }

      prevActualTickCount_ = ticks.Length;

      if(labels != null) {
        for(var i_ = 0; i_ < labels.Length; i_++) {
          labelProvider.ReleaseLabel(label: labels[i_]);
        }
      }

      labels = labelProvider.CreateLabels(ticksInfo: ticksInfo);

      var prevResult_ = result_;
      result_ = CheckLabelsArrangement(labels: labels, ticks: ticks);

      if(prevResult_ == TickCountChange.Decrease && result_ == TickCountChange.Increase) {
        // stop tick number oscillating
        result_ = TickCountChange.Ok;
      }

      if(result_ != TickCountChange.Ok) {
        var prevTickCount_ = tickCount_;
        tickCount_ = result_ == TickCountChange.Decrease
          ? ticksProvider.DecreaseTickCount(ticksCount: tickCount_)
          : ticksProvider.IncreaseTickCount(ticksCount: tickCount_);

        //DebugVerify.Is(tickCount >= prevTickCount);
        // ticks provider could not create less ticks or tick number didn't change
        if(tickCount_ == 0 || prevTickCount_ == tickCount_) {
          tickCount_ = prevTickCount_;
          result_ = TickCountChange.Ok;
        }
      }
    } while(result_ != TickCountChange.Ok);

    previousTickCount = tickCount_;
  }

  private TickCountChange CheckLabelsArrangement(UIElement[] labels, T[] ticks) {
    var actualLabels_ = labels.Select(selector: (label, i) => new { Label = label, Index = i })
      .Where(predicate: el => el.Label != null)
      .Select(selector: el => new { el.Label, Tick = ticks[el.Index] })
      .ToList();

    actualLabels_.ForEach(action: item => item.Label.Measure(availableSize: RenderSize));

    var sizeInfos_ = actualLabels_.Select(selector: item =>
        new { X = GetCoordinateFromTick(tick: item.Tick), Size = getSize(arg: item.Label.DesiredSize) })
      .OrderBy(keySelector: item => item.X).ToArray();

    var res_ = TickCountChange.Ok;

    var increaseCount_ = 0;
    for(var i_ = 0; i_ < sizeInfos_.Length - 1; i_++) {
      if(sizeInfos_[i_].X + sizeInfos_[i_].Size * DecreaseRatio > sizeInfos_[i_ + 1].X) {
        res_ = TickCountChange.Decrease;
        break;
      }

      if(sizeInfos_[i_].X + sizeInfos_[i_].Size * IncreaseRatio < sizeInfos_[i_ + 1].X) {
        increaseCount_++;
      }
    }

    if(increaseCount_ > sizeInfos_.Length / 2) {
      res_ = TickCountChange.Increase;
    }

    return res_;
  }
}

[DebuggerDisplay(value: "{X} + {Size}")]
internal sealed class SizeInfo : IComparable<SizeInfo> {
  public double Size { get; set; }
  public double X { get; set; }


  public int CompareTo(SizeInfo other) {
    return X.CompareTo(value: other.X);
  }
}

internal enum TickCountChange {
  Increase = -1,
  Ok = 0,
  Decrease = 1
}

/// <summary>
/// Represents an auxiliary structure for storing additional info during major DateTime labels generation.
/// </summary>
public struct MajorLabelsInfo {
  public object Info { get; set; }
  public int MajorLabelsCount { get; set; }

  public override string ToString() {
    return string.Format(format: "{0}, Count={1}", arg0: Info, arg1: MajorLabelsCount);
  }
}
