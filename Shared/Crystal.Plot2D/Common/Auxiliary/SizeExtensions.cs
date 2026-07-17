using System;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class SizeExtensions {
  private const double SizeRatio = 1e-7;

  public static bool EqualsApproximately(this Size size1, Size size2) {
    var widthEquals_ = Math.Abs(value: size1.Width - size2.Width) / size1.Width < SizeRatio;
    var heightEquals_ = Math.Abs(value: size1.Height - size2.Height) / size1.Height < SizeRatio;

    return widthEquals_ && heightEquals_;
  }
}
