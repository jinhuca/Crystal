using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Crystal.Controls.Borders;

/// <summary>
/// A rounded panel with a soft shadow cast <i>inward</i> from its edges, giving a recessed / inset look.
///
/// The effect is a blurred rim: the template lays a thick, blurred border on top of the background,
/// then this control clips that rim to the panel's rounded bounds. Clipping removes the half of the
/// blur that would bleed outward, leaving only the inward falloff — darkest at the edge, fading to
/// the panel color toward the center. Use a dark, semi-transparent <see cref="ShadowBrush"/> for a
/// shadow; a light one turns the same rim into an inner glow.
/// </summary>
public class InnerShadowBorder : ContentControl {
  /// <summary>
  /// Initializes the <see cref="InnerShadowBorder"/> class, overriding the default style key to associate 
  /// it with its control template.
  /// </summary>
  static InnerShadowBorder() {
    DefaultStyleKeyProperty.OverrideMetadata(
      typeof(InnerShadowBorder), new FrameworkPropertyMetadata(typeof(InnerShadowBorder)));
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="InnerShadowBorder"/> class, setting up event handlers
  /// </summary>
  public InnerShadowBorder() {
    // Track hardware/software transitions at runtime (e.g. an RDP session connecting or
    // dropping) so the CPU-costly blur is disabled/re-enabled to match.
    Loaded += (_, _) => RenderCapability.TierChanged += OnTierChanged;
    Unloaded += (_, _) => RenderCapability.TierChanged -= OnTierChanged;
  }

  /// <summary>
  /// The template part whose rendering (the blurred rim) we clip to the rounded rectangle.
  /// </summary>
  private FrameworkElement? _shadowClip;

  /// <summary>
  /// The blurred rim border and its BlurEffect, captured so we can drop/restore the effect based
  /// on the render tier without losing the (data-bound) effect instance.
  /// </summary>
  private Border? _shadowBorder;

  /// <summary>
  /// The BlurEffect applied to the shadow border, captured so we can drop/restore it based on the render tier.
  /// </summary>
  private Effect? _blurEffect;

  /// <summary>
  /// Gets or sets the corner radius of the panel, which also defines the rounded rectangle used to clip the blurred rim.
  /// </summary>
  public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
    nameof(CornerRadius),
    typeof(CornerRadius),
    typeof(InnerShadowBorder),
    new FrameworkPropertyMetadata(new CornerRadius(10), FrameworkPropertyMetadataOptions.AffectsRender, OnClipInvalidated));

  /// <summary>
  /// Gets or sets the corner radius of the panel, which also defines the rounded rectangle used to clip the blurred rim.
  /// </summary>
  public CornerRadius CornerRadius {
    get => (CornerRadius)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  /// <summary>
  /// Color of the inner shadow rim. Dark + semi-transparent reads as a shadow.
  /// </summary>
  public static readonly DependencyProperty ShadowBrushProperty = DependencyProperty.Register(
    nameof(ShadowBrush),
    typeof(Brush),
    typeof(InnerShadowBorder),
    new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xB0, 0x00, 0x00, 0x00))));

  /// <summary>
  /// Color of the inner shadow rim. Dark + semi-transparent reads as a shadow.
  /// </summary>
  public Brush ShadowBrush {
    get => (Brush)GetValue(ShadowBrushProperty);
    set => SetValue(ShadowBrushProperty, value);
  }

  /// <summary>
  /// How far the shadow band reaches in from the edge before the blur softens it.
  /// </summary>
  public static readonly DependencyProperty ShadowSpreadProperty = DependencyProperty.Register(
    nameof(ShadowSpread),
    typeof(double),
    typeof(InnerShadowBorder),
    new PropertyMetadata(8.0));

  /// <summary>
  /// How far the shadow band reaches in from the edge before the blur softens it.
  /// </summary>
  public double ShadowSpread {
    get => (double)GetValue(ShadowSpreadProperty);
    set => SetValue(ShadowSpreadProperty, value);
  }

  /// <summary>
  /// Blur radius applied to the rim — larger values give a softer, wider shadow.
  /// </summary>
  public static readonly DependencyProperty ShadowBlurRadiusProperty = DependencyProperty.Register(
    nameof(ShadowBlurRadius),
    typeof(double),
    typeof(InnerShadowBorder),
    new PropertyMetadata(14.0));

  /// <summary>
  /// Blur radius applied to the rim — larger values give a softer, wider shadow.
  /// </summary>
  public double ShadowBlurRadius {
    get => (double)GetValue(ShadowBlurRadiusProperty);
    set => SetValue(ShadowBlurRadiusProperty, value);
  }

  /// <summary>
  /// Fill of the panel behind the shadow and content.
  /// </summary>
  public static readonly DependencyProperty PanelBackgroundProperty = DependencyProperty.Register(
    nameof(PanelBackground),
    typeof(Brush),
    typeof(InnerShadowBorder),
    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x22, 0x24, 0x29))));

  /// <summary>
  /// Fill of the panel behind the shadow and content.
  /// </summary>
  public Brush PanelBackground {
    get => (Brush)GetValue(PanelBackgroundProperty);
    set => SetValue(PanelBackgroundProperty, value);
  }

  /// <summary>
  /// Crisp 1px outline drawn under the shadow.
  /// </summary>
  public static readonly DependencyProperty PanelBorderBrushProperty = DependencyProperty.Register(
    nameof(PanelBorderBrush),
    typeof(Brush),
    typeof(InnerShadowBorder),
    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3A, 0x3D, 0x44))));

  /// <summary>
  /// Crisp 1px outline drawn under the shadow.
  /// </summary>
  public Brush PanelBorderBrush {
    get => (Brush)GetValue(PanelBorderBrushProperty);
    set => SetValue(PanelBorderBrushProperty, value);
  }

  /// <summary>
  /// Called when the control's template is applied. Captures references to the shadow clip and 
  /// border elements, applies the appropriate render tier settings, and updates the clipping geometry 
  /// for the blurred rim.
  /// </summary>
  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    _shadowClip = GetTemplateChild("PART_ShadowClip") as FrameworkElement;
    _shadowBorder = GetTemplateChild("PART_ShadowBorder") as Border;
    _blurEffect = _shadowBorder?.Effect;
    ApplyRenderTier();
    UpdateClip();
  }

  /// <summary>
  /// Called when the render tier changes (e.g., due to hardware acceleration availability).
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnTierChanged(object? sender, EventArgs e) => ApplyRenderTier();

  /// <summary>
  /// Keep the GPU blur only when hardware acceleration is available. Under software rendering a
  /// large Gaussian blur runs on the CPU and can stall the UI, so we drop the effect — the crisp
  /// inset band still reads as recessed, just without the soft falloff.
  /// </summary>
  private void ApplyRenderTier() {
    if(_shadowBorder is null) return;
    _shadowBorder.Effect = RenderTierInfo.IsHardwareAccelerated ? _blurEffect : null;
  }

  /// <summary>
  /// Called when the control's size changes. Updates the clipping geometry for the blurred rim to match the new size.
  /// </summary>
  /// <param name="sizeInfo">Information about the size change.</param>
  protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
    base.OnRenderSizeChanged(sizeInfo);
    UpdateClip();
  }

  /// <summary>
  /// Called when a dependency property that affects the clipping geometry changes (e.g., CornerRadius). 
  /// Updates the clipping geometry for the blurred rim.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnClipInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    => ((InnerShadowBorder)d).UpdateClip();

  /// <summary>
  /// Clip the blurred rim to the panel's rounded rectangle so only the inward shadow survives.
  /// </summary>
  private void UpdateClip() {
    if(_shadowClip is null) return;

    double w = ActualWidth, h = ActualHeight;
    if(w <= 0 || h <= 0) {
      _shadowClip.Clip = null;
      return;
    }

    double radius = Math.Min(CornerRadius.TopLeft, Math.Min(w, h) / 2);
    _shadowClip.Clip = new RectangleGeometry(new Rect(0, 0, w, h), radius, radius);
  }
}