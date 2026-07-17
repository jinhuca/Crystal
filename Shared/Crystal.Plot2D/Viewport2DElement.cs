using Crystal.Plot2D.Common;
using Crystal.Plot2D.Transforms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Crystal.Plot2D;

/// <summary>
/// Viewport2DElement is intended to be a child of Viewport2D. 
/// Specifics of Viewport2DElement is Viewport2D attached property.
/// </summary>
public abstract class Viewport2DElement : FrameworkElement, IPlotterElement, INotifyPropertyChanged {
  protected Panel GetHostPanel(PlotterBase plotter) => plotter.CentralGrid;

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) => OnPlotterAttached(plotter: plotter);
  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) => OnPlotterDetaching(plotter: plotter);
  public PlotterBase Plotter { get; private set; }

  protected virtual void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    GetHostPanel(plotter: plotter).Children.Add(element: this);
    Viewport = Plotter.Viewport;
    Viewport.PropertyChanged += OnViewportPropertyChanged;
  }

  private void OnViewportPropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    switch(e.PropertyName) {
      case "Visible":
        OnVisibleChanged(newRect: (DataRect)e.NewValue, oldRect: (DataRect)e.OldValue);
        break;
      case "Output":
        OnOutputChanged(newRect: (Rect)e.NewValue, oldRect: (Rect)e.OldValue);
        break;
      case "Transform":
        Update();
        break;
    }
  }

  protected virtual void OnPlotterDetaching(PlotterBase plotter) {
    Viewport.PropertyChanged -= OnViewportPropertyChanged;
    Viewport = null;
    GetHostPanel(plotter: plotter).Children.Remove(element: this);
    Plotter = null;
  }

  #endregion IPlotterElement Members

  public int ZIndex {
    get => Panel.GetZIndex(element: this);
    set => Panel.SetZIndex(element: this, value: value);
  }

  #region Viewport

  protected Viewport2D Viewport { get; private set; }

  #endregion

  protected internal Vector Offset { get; set; }

  protected virtual void OnVisibleChanged(DataRect newRect, DataRect oldRect) {
    if(newRect.Size == oldRect.Size) {
      var transform_ = Viewport.Transform;
      Offset += oldRect.Location.DataToScreen(transform: transform_) - newRect.Location.DataToScreen(transform: transform_);
      if(ManualTranslate) {
        Update();
      }
    }
    else {
      Offset = new Vector();
      Update();
    }
  }

  protected virtual void OnOutputChanged(Rect newRect, Rect oldRect) {
    Offset = new Vector();
    Update();
  }

  /// <summary>
  ///   Gets a value indicating whether this instance is translated.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if this instance is translated; otherwise, <c>false</c>.
  /// </value>
  protected bool IsTranslated => Offset.X != 0 || Offset.Y != 0;

  #region IsLevel

  public bool IsLayer {
    get => (bool)GetValue(dp: IsLayerProperty);
    set => SetValue(dp: IsLayerProperty, value: value);
  }

  public static readonly DependencyProperty IsLayerProperty = DependencyProperty.Register(
    name: nameof(IsLayer),
    propertyType: typeof(bool),
    ownerType: typeof(Viewport2DElement),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion

  #region Rendering & caching options

  protected object GetValueSync(DependencyProperty property) =>
    Dispatcher.Invoke(
      priority: DispatcherPriority.Send,
      method: (DispatcherOperationCallback)delegate { return GetValue(dp: property); },
      arg: property);

  protected void SetValueAsync(DependencyProperty property, object value)
    => Dispatcher.BeginInvoke(
      priority: DispatcherPriority.Send,
      method: (SendOrPostCallback)delegate { SetValue(dp: property, value: value); },
      arg: value);

  private bool manualClip;

  /// <summary>
  /// Gets or sets a value indicating whether descendant graph class relies on automatic clipping
  /// by Viewport.Output or does its own clipping.
  /// </summary>
  public bool ManualClip {
    get => manualClip;
    set => manualClip = value;
  }

  private bool manualTranslate;
  /// <summary>
  ///   Gets or sets a value indicating whether descendant graph class relies on automatic translation of it, or does its own.
  /// </summary>
  public bool ManualTranslate {
    get => manualTranslate;
    set => manualTranslate = value;
  }

  private RenderTo renderTarget = RenderTo.Screen;
  /// <summary>
  ///   Gets or sets a value indicating whether descendant graph class uses cached rendering of its content to image, or not.
  /// </summary>
  public RenderTo RenderTarget {
    get => renderTarget;
    set => renderTarget = value;
  }

  private enum ImageKind {
    Real,
    BeingRendered,
    Empty
  }

  #endregion

  private RenderState CreateRenderState(DataRect renderVisible, RenderTo renderingType)
    => new(renderVisible: renderVisible, visible: Viewport.Visible, output: Viewport.Output, renderingType: renderingType);

  protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
    base.OnPropertyChanged(e: e);
    if(e.Property == VisibilityProperty) {
      Update();
    }
  }

  private bool updateCalled;
  private bool beforeFirstUpdate = true;

  protected void Update() {
    if(Viewport == null) {
      return;
    }

    UpdateCore();
    if(!beforeFirstUpdate) {
      updateCalled = true;
      InvalidateVisual();
    }

    beforeFirstUpdate = false;
  }

  protected virtual void UpdateCore() { }

  protected void TranslateVisual() {
    if(!ManualTranslate) {
      shouldReRender = false;
    }

    InvalidateVisual();
  }

  #region Thumbnail

  private ImageSource thumbnail;

  public ImageSource Thumbnail {
    get {
      if(!CreateThumbnail) {
        CreateThumbnail = true;
      }

      return thumbnail;
    }
  }

  private bool createThumbnail;

  public bool CreateThumbnail {
    get => createThumbnail;
    set {
      if(createThumbnail != value) {
        createThumbnail = value;
        if(value) {
          RenderThumbnail();
        }
        else {
          thumbnail = null;
          RaisePropertyChanged(propertyName: nameof(Thumbnail));
        }
      }
    }
  }

  private bool ShouldCreateThumbnail => IsLayer && createThumbnail;

  private void RenderThumbnail() {
    if(Viewport == null) {
      return;
    }

    var output_ = Viewport.Output;
    if(output_.Width == 0 || output_.Height == 0) {
      return;
    }

    var visible_ = Viewport.Visible;
    var transform_ = Viewport.Transform;
    DrawingVisual visual_ = new();
    using(var dc_ = visual_.RenderOpen()) {
      var outputStart_ = visible_.Location.DataToScreen(transform: transform_);
      var x_ = -outputStart_.X + Offset.X;
      var y_ = -outputStart_.Y + output_.Bottom - output_.Top + Offset.Y;
      var translate_ = !manualTranslate && IsTranslated;
      if(translate_) {
        dc_.PushTransform(transform: new TranslateTransform(offsetX: x_, offsetY: y_));
      }

      const byte c = 240;
      Brush brush_ = new SolidColorBrush(color: Color.FromArgb(a: 120, r: c, g: c, b: c));
      Pen pen_ = new(brush: Brushes.Black, thickness: 1);
      dc_.DrawRectangle(brush: brush_, pen: pen_, rectangle: output_);
      dc_.DrawDrawing(drawing: graphContents);

      if(translate_) {
        dc_.Pop();
      }
    }

    RenderTargetBitmap bmp_ = new(pixelWidth: (int)output_.Width, pixelHeight: (int)output_.Height, dpiX: 96, dpiY: 96, pixelFormat: PixelFormats.Pbgra32);
    bmp_.Render(visual: visual_);
    thumbnail = bmp_;
    RaisePropertyChanged(propertyName: nameof(Thumbnail));
  }

  #endregion

  private bool shouldReRender = true;
  private DrawingGroup graphContents;
  protected sealed override void OnRender(DrawingContext drawingContext) {
    if(Viewport == null) {
      return;
    }

    var output_ = Viewport.Output;
    if(output_.Width == 0 || output_.Height == 0) {
      return;
    }
    if(output_.IsEmpty) {
      return;
    }
    if(Visibility != Visibility.Visible) {
      return;
    }
    if(shouldReRender || manualTranslate || renderTarget == RenderTo.Image || beforeFirstUpdate || updateCalled) {
      graphContents ??= new DrawingGroup();
      if(beforeFirstUpdate) {
        Update();
      }

      using(var context_ = graphContents.Open()) {
        if(renderTarget == RenderTo.Screen) {
          var state_ = CreateRenderState(renderVisible: Viewport.Visible, renderingType: RenderTo.Screen);
          OnRenderCore(dc: context_, state: state_);
        }
      }
      updateCalled = false;
    }

    // thumbnail is not created, if
    // 1) CreateThumbnail is false
    // 2) this graph has IsLayer property, set to false
    if(ShouldCreateThumbnail) {
      RenderThumbnail();
    }

    if(!manualClip) {
      drawingContext.PushClip(clipGeometry: new RectangleGeometry(rect: output_));
    }
    var translate_ = !manualTranslate && IsTranslated;
    if(translate_) {
      drawingContext.PushTransform(transform: new TranslateTransform(offsetX: Offset.X, offsetY: Offset.Y));
    }

    drawingContext.DrawDrawing(drawing: graphContents);

    if(translate_) {
      drawingContext.Pop();
    }
    if(!manualClip) {
      drawingContext.Pop();
    }
    shouldReRender = true;
  }

  protected abstract void OnRenderCore(DrawingContext dc, RenderState state);

  #region INotifyPropertyChanged Members

  public event PropertyChangedEventHandler PropertyChanged;

  protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    => PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(propertyName: propertyName));

  #endregion
}