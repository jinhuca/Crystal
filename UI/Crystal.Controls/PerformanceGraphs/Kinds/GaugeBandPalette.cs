using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

// The one green→red gauge ramp shared by every value-banded renderer: PerformanceGraphLite's
// dot matrix (row-by-value coloring) and PerformanceGraph's banded Line kind (segment-by-value
// coloring). Kept in one place so the two render paths read as the same low-to-high palette.
internal static class GaugeBandPalette {
  public const int BandCount = 9;

  // Alpha used for the translucent area-fill variant of each band. Lower than the solid stroke so
  // the stacked fill under a banded line reads as a soft gauge backdrop, not nine hard color blocks.
  private const byte FillAlpha = 0x5A;

  private static readonly (byte R, byte G, byte B)[] Ramp = {
    (0x2E, 0xCC, 0x40), // Band 0 - green
    (0x5F, 0xCA, 0x34),
    (0x90, 0xC8, 0x28),
    (0xC0, 0xC6, 0x1B),
    (0xF1, 0xC4, 0x0F), // Band 4 - yellow
    (0xF0, 0x9A, 0x14),
    (0xEF, 0x70, 0x1A),
    (0xEE, 0x46, 0x1F),
    (0xED, 0x1C, 0x24), // Band 8 - red
  };

  // Solid band colors (Band 0 green … Band 8 red), frozen once and shared. An unconfigured graph
  // reads as a linear low-to-high palette rather than flat gray. Declared after Ramp so the static
  // initializer sees it populated (C# runs field initializers in textual order).
  public static readonly Brush[] Solid = CreateSolid();

  // The same colors at reduced alpha, for painting the area under a banded line.
  public static readonly Brush[] Fill = CreateFill();

  private static Brush[] CreateSolid() {
    var brushes = new Brush[Ramp.Length];
    for (int i = 0; i < Ramp.Length; i++) {
      var brush = new SolidColorBrush(Color.FromRgb(Ramp[i].R, Ramp[i].G, Ramp[i].B));
      brush.Freeze();
      brushes[i] = brush;
    }
    return brushes;
  }

  private static Brush[] CreateFill() {
    var brushes = new Brush[Ramp.Length];
    for (int i = 0; i < Ramp.Length; i++) {
      var brush = new SolidColorBrush(Color.FromArgb(FillAlpha, Ramp[i].R, Ramp[i].G, Ramp[i].B));
      brush.Freeze();
      brushes[i] = brush;
    }
    return brushes;
  }

  // A BandCount-length solid ramp linearly interpolated (in RGB) between two endpoint colors, so a
  // caller that only knows "low color → high color" (e.g. DodgerBlue → Red) gets the same nine-band
  // structure the built-in green→red ramp uses, per-instance, without hand-picking nine colors. Band
  // 0 is start, band BandCount-1 is end. Frozen, so the result is safe to share and cache.
  public static Brush[] BuildSolidRamp(Color start, Color end) {
    var brushes = new Brush[BandCount];
    for (int i = 0; i < BandCount; i++) {
      double t = BandCount == 1 ? 0 : i / (double)(BandCount - 1);
      var brush = new SolidColorBrush(Color.FromRgb(Lerp(start.R, end.R, t), Lerp(start.G, end.G, t), Lerp(start.B, end.B, t)));
      brush.Freeze();
      brushes[i] = brush;
    }
    return brushes;
  }

  // The translucent area-fill counterpart of a custom solid ramp: each solid band's color re-emitted
  // at FillAlpha, matching how Fill relates to Solid for the built-in ramp. Non-solid brushes fall
  // back to fully transparent (this control's bands are always solid colors in practice). Frozen.
  public static Brush[] DeriveFill(Brush[] solids) {
    var brushes = new Brush[solids.Length];
    for (int i = 0; i < solids.Length; i++) {
      Color c = solids[i] is SolidColorBrush s ? s.Color : Colors.Transparent;
      var brush = new SolidColorBrush(Color.FromArgb(FillAlpha, c.R, c.G, c.B));
      brush.Freeze();
      brushes[i] = brush;
    }
    return brushes;
  }

  private static byte Lerp(byte a, byte b, double t) => (byte)Math.Round(a + (b - a) * t);
}
