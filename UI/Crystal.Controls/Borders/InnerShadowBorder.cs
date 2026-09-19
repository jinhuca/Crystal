using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Crystal.Controls.Borders;

/// <summary>
/// A rounded panel with a soft shadow cast <i>inward</i> from its edges, giving a recessed / inset
/// look.
///
/// The effect is a blurred rim: the template lays a thick, blurred border on top of the background,
/// then this control clips that rim to the panel's rounded bounds. Clipping removes the half of the
/// blur that would bleed outward, leaving only the inward falloff — darkest at the edge, fading to
/// the panel color toward the center. Use a dark, semi-transparent <see cref="ShadowBrush"/> for a
/// shadow; a light one turns the same rim into an inner glow.
/// </summary>
public class InnerShadowBorder : ContentControl {
  static InnerShadowBorder() {
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(InnerShadowBorder), new FrameworkPropertyMetadata(typeof(InnerShadowBorder)));
  }

  public InnerShadowBorder() {
    // Track hardware/software transitions at runtime (e.g. an RDP session connecting or
    // dropping) so the CPU-costly blur is disabled/re-enabled to match.
    Loaded += (_, _) => RenderCapability.TierChanged += OnTierChanged;
    Unloaded += (_, _) => RenderCapability.TierChanged -= OnTierChanged;
  }

  // The template part whose rendering (the blurred rim) we clip to the rounded rectangle.
  private FrameworkElement? _shadowClip;

  // The blurred rim border and its BlurEffect, captured so we can drop/restore the effect based
  // on the render tier without losing the (data-bound) effect instance.
  private Border? _shadowBorder;
  private Effect? _blurEffect;

  public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
      nameof(CornerRadius), typeof(CornerRadius), typeof(InnerShadowBorder),
      new FrameworkPropertyMetadata(new CornerRadius(10), FrameworkPropertyMetadataOptions.AffectsRender, OnClipInvalidated));

  public CornerRadius CornerRadius {
    get => (CornerRadius)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  /// <summary>Color of the inner shadow rim. Dark + semi-transparent reads as a shadow.</summary>
  public static readonly DependencyProperty ShadowBrushProperty = DependencyProperty.Register(
      nameof(ShadowBrush), typeof(Brush), typeof(InnerShadowBorder),
      new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xB0, 0x00, 0x00, 0x00))));

  public Brush ShadowBrush {
    get => (Brush)GetValue(ShadowBrushProperty);
    set => SetValue(ShadowBrushProperty, value);
  }

  /// <summary>How far the shadow band reaches in from the edge before the blur softens it.</summary>
  public static readonly DependencyProperty ShadowSpreadProperty = DependencyProperty.Register(
      nameof(ShadowSpread), typeof(double), typeof(InnerShadowBorder),
      new PropertyMetadata(8.0));

  public double ShadowSpread {
    get => (double)GetValue(ShadowSpreadProperty);
    set => SetValue(ShadowSpreadProperty, value);
  }

  /// <summary>Blur radius applied to the rim — larger values give a softer, wider shadow.</summary>
  public static readonly DependencyProperty ShadowBlurRadiusProperty = DependencyProperty.Register(
      nameof(ShadowBlurRadius), typeof(double), typeof(InnerShadowBorder),
      new PropertyMetadata(14.0));

  public double ShadowBlurRadius {
    get => (double)GetValue(ShadowBlurRadiusProperty);
    set => SetValue(ShadowBlurRadiusProperty, value);
  }

  /// <summary>Fill of the panel behind the shadow and content.</summary>
  public static readonly DependencyProperty PanelBackgroundProperty = DependencyProperty.Register(
      nameof(PanelBackground), typeof(Brush), typeof(InnerShadowBorder),
      new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x22, 0x24, 0x29))));

  public Brush PanelBackground {
    get => (Brush)GetValue(PanelBackgroundProperty);
    set => SetValue(PanelBackgroundProperty, value);
  }

  /// <summary>Crisp 1px outline drawn under the shadow.</summary>
  public static readonly DependencyProperty PanelBorderBrushProperty = DependencyProperty.Register(
      nameof(PanelBorderBrush), typeof(Brush), typeof(InnerShadowBorder),
      new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3A, 0x3D, 0x44))));

  public Brush PanelBorderBrush {
    get => (Brush)GetValue(PanelBorderBrushProperty);
    set => SetValue(PanelBorderBrushProperty, value);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    _shadowClip = GetTemplateChild("PART_ShadowClip") as FrameworkElement;
    _shadowBorder = GetTemplateChild("PART_ShadowBorder") as Border;
    _blurEffect = _shadowBorder?.Effect;
    ApplyRenderTier();
    UpdateClip();
  }

  private void OnTierChanged(object? sender, EventArgs e) => ApplyRenderTier();

  // Keep the GPU blur only when hardware acceleration is available. Under software rendering a
  // large Gaussian blur runs on the CPU and can stall the UI, so we drop the effect — the crisp
  // inset band still reads as recessed, just without the soft falloff.
  private void ApplyRenderTier() {
    if(_shadowBorder is null) return;
    _shadowBorder.Effect = RenderTierInfo.IsHardwareAccelerated ? _blurEffect : null;
  }

  protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
    base.OnRenderSizeChanged(sizeInfo);
    UpdateClip();
  }

  private static void OnClipInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
      => ((InnerShadowBorder)d).UpdateClip();

  // Clip the blurred rim to the panel's rounded rectangle so only the inward shadow survives.
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