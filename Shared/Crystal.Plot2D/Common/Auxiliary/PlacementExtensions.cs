using Crystal.Plot2D.Axes;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class PlacementExtensions {
  public static bool IsBottomOrTop(this AxisPlacement placement) {
    return placement == AxisPlacement.Bottom || placement == AxisPlacement.Top;
  }
}
