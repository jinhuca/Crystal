using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Shell.Views;

/// <summary>
/// A dark, in-app colour picker matching the shell's chrome (a replacement for the mismatched Win32
/// dialog): a hue×saturation field with a brightness slider, plus hex and RGB inputs and a preview.
/// State is held as HSV so the field/slider map directly to a coordinate; the RGB/hex fields convert
/// on the way in and out.
/// </summary>
public partial class ColorPickerDialog : Window {
  private double _hue;   // 0..360
  private double _sat;   // 0..1
  private double _val;   // 0..1
  private bool _syncing; // guards field round-tripping while we rewrite the boxes

  public ColorPickerDialog(Color initial) {
    InitializeComponent();
    (_hue, _sat, _val) = ColorMath.RgbToHsv(initial);
    Loaded += (_, _) => { RefreshAll(); PositionThumbs(); };
  }

  /// <summary>The colour currently described by the picker.</summary>
  public Color SelectedColor => ColorMath.HsvToRgb(_hue, _sat, _val);

  // --- Hue×Saturation field -------------------------------------------------------------------

  private void OnSvMouseDown(object sender, MouseButtonEventArgs e) {
    SvArea.CaptureMouse();
    UpdateSvFromPoint(e.GetPosition(SvArea));
  }

  private void OnSvMouseMove(object sender, MouseEventArgs e) {
    if (e.LeftButton == MouseButtonState.Pressed && SvArea.IsMouseCaptured)
      UpdateSvFromPoint(e.GetPosition(SvArea));
  }

  private void OnSvMouseUp(object sender, MouseButtonEventArgs e) => SvArea.ReleaseMouseCapture();

  private void UpdateSvFromPoint(Point p) {
    double w = SvArea.ActualWidth, h = SvArea.ActualHeight;
    if (w <= 0 || h <= 0) return;
    _hue = Clamp01(p.X / w) * 360.0;
    _sat = 1.0 - Clamp01(p.Y / h);
    RefreshAll();
    PositionThumbs();
  }

  // --- Brightness slider ----------------------------------------------------------------------

  private void OnValueMouseDown(object sender, MouseButtonEventArgs e) {
    ValueBar.CaptureMouse();
    UpdateValueFromPoint(e.GetPosition(ValueBar));
  }

  private void OnValueMouseMove(object sender, MouseEventArgs e) {
    if (e.LeftButton == MouseButtonState.Pressed && ValueBar.IsMouseCaptured)
      UpdateValueFromPoint(e.GetPosition(ValueBar));
  }

  private void OnValueMouseUp(object sender, MouseButtonEventArgs e) => ValueBar.ReleaseMouseCapture();

  private void UpdateValueFromPoint(Point p) {
    double h = ValueBar.ActualHeight;
    if (h <= 0) return;
    _val = 1.0 - Clamp01(p.Y / h);
    RefreshAll();
    PositionThumbs();
  }

  // --- Hex / RGB inputs -----------------------------------------------------------------------

  private void OnHexKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) OnHexCommitted(sender, e);
  }

  private void OnHexCommitted(object sender, RoutedEventArgs e) {
    if (_syncing) return;
    var text = HexBox.Text.Trim();
    if (!text.StartsWith('#')) text = "#" + text;
    try {
      var c = (Color)ColorConverter.ConvertFromString(text);
      (_hue, _sat, _val) = ColorMath.RgbToHsv(c);
      RefreshAll();          // reformats the hex box to the canonical form
      PositionThumbs();
    } catch {
      RefreshHex();          // reject: restore the last good value
    }
  }

  private void OnRgbChanged(object sender, TextChangedEventArgs e) {
    if (_syncing) return;
    byte r = ParseByte(RedBox.Text), g = ParseByte(GreenBox.Text), b = ParseByte(BlueBox.Text);
    (_hue, _sat, _val) = ColorMath.RgbToHsv(Color.FromRgb(r, g, b));
    // Leave the RGB boxes as typed; sync only the hex box and the visuals.
    RefreshHex();
    RefreshVisuals();
    PositionThumbs();
  }

  // --- OK / Cancel ----------------------------------------------------------------------------

  private void OnOkClick(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
  private void OnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

  // --- Refresh --------------------------------------------------------------------------------

  private void RefreshAll() { RefreshVisuals(); RefreshHex(); RefreshRgb(); }

  private void RefreshVisuals() {
    var color = SelectedColor;
    PreviewSwatch.Fill = new SolidColorBrush(color);
    SvValueOverlay.Opacity = 1.0 - _val;
    // The slider shows the current hue/saturation from full brightness (top) to black (bottom).
    ValueBarFill.Fill = new LinearGradientBrush(ColorMath.HsvToRgb(_hue, _sat, 1.0), Colors.Black, 90);
  }

  private void RefreshHex() {
    _syncing = true;
    HexBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
    _syncing = false;
  }

  private void RefreshRgb() {
    var c = SelectedColor;
    _syncing = true;
    RedBox.Text = c.R.ToString(CultureInfo.InvariantCulture);
    GreenBox.Text = c.G.ToString(CultureInfo.InvariantCulture);
    BlueBox.Text = c.B.ToString(CultureInfo.InvariantCulture);
    _syncing = false;
  }

  private void PositionThumbs() {
    double w = SvArea.ActualWidth, h = SvArea.ActualHeight;
    if (w > 0 && h > 0) {
      Canvas.SetLeft(SvThumb, (_hue / 360.0) * w - SvThumb.Width / 2);
      Canvas.SetTop(SvThumb, (1.0 - _sat) * h - SvThumb.Height / 2);
    }
    double bh = ValueBar.ActualHeight;
    if (bh > 0) Canvas.SetTop(ValueThumb, (1.0 - _val) * bh - ValueThumb.Height / 2);
  }

  // --- Helpers --------------------------------------------------------------------------------

  private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

  private static byte ParseByte(string s) =>
      byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b) ? b
      : int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? (byte)Math.Clamp(i, 0, 255)
      : (byte)0;
}
