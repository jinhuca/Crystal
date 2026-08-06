using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystal.Controls.Tests;

/// <summary>
/// Renders a <see cref="FrameworkElement"/> to an off-screen bitmap and exposes simple pixel
/// queries, so tests can assert on what the control actually draws (exercising OnRender and the
/// internal renderers end-to-end) without needing access to internal types or a DrawingContext.
/// Must be called on an STA thread — see <see cref="StaRunner"/>.
/// </summary>
internal sealed class PixelRenderer {
  private readonly byte[] _pixels; // BGRA, 4 bytes per pixel
  private readonly int _stride;

  public int Width { get; }
  public int Height { get; }

  public PixelRenderer(FrameworkElement element, int width, int height) {
    Width = width;
    Height = height;
    _stride = width * 4;

    var size = new Size(width, height);
    element.Measure(size);
    element.Arrange(new Rect(size));
    element.UpdateLayout();

    var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
    bitmap.Render(element);

    _pixels = new byte[height * _stride];
    bitmap.CopyPixels(_pixels, _stride, 0);
  }

  /// <summary>Number of pixels whose RGB matches <paramref name="color"/> within <paramref name="tolerance"/> per channel.</summary>
  public int CountColor(Color color, int tolerance = 20) {
    int count = 0;
    for (int i = 0; i < _pixels.Length; i += 4) {
      if (Matches(i, color, tolerance)) count++;
    }
    return count;
  }

  /// <summary>Highest row index (0 = top) at which any pixel matches <paramref name="color"/>, or -1 if none.</summary>
  public int TopMostRowWithColor(Color color, int tolerance = 20) {
    for (int y = 0; y < Height; y++) {
      for (int x = 0; x < Width; x++) {
        if (Matches(y * _stride + x * 4, color, tolerance)) return y;
      }
    }
    return -1;
  }

  public Color At(int x, int y) {
    int i = y * _stride + x * 4;
    return Color.FromRgb(_pixels[i + 2], _pixels[i + 1], _pixels[i]);
  }

  private bool Matches(int offset, Color color, int tolerance) =>
      Math.Abs(_pixels[offset] - color.B) <= tolerance &&
      Math.Abs(_pixels[offset + 1] - color.G) <= tolerance &&
      Math.Abs(_pixels[offset + 2] - color.R) <= tolerance;
}
