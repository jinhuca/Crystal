using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Common.Palettes;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D.Charts;

public class NaiveColorMap {
  public NaiveColorMap(double[,] data, IPalette palette) {
    Data = data;
    Palette = palette;
  }

  public double[,] Data { get; }

  public IPalette Palette { get; }

  public BitmapSource BuildImage() {
    if(Data == null) {
      throw new ArgumentNullException(paramName: nameof(Data));
    }

    if(Palette == null) {
      throw new ArgumentNullException(paramName: nameof(Palette));
    }

    var width_ = Data.GetLength(dimension: 0);
    var height_ = Data.GetLength(dimension: 1);

    var pixels_ = new int[width_ * height_];
    var minMax_ = Data.GetMinMax();
    var min_ = minMax_.Min;
    var rangeDelta_ = minMax_.GetLength();

    var pointer_ = 0;
    for(var iy_ = 0; iy_ < height_; iy_++) {
      for(var ix_ = 0; ix_ < width_; ix_++) {
        var value_ = Data[ix_, height_ - 1 - iy_];
        var ratio_ = (value_ - min_) / rangeDelta_;
        var color_ = Palette.GetColor(t: ratio_);
        var argb_ = color_.ToArgb();
        pixels_[pointer_++] = argb_;
      }
    }

    WriteableBitmap bitmap_ = new(pixelWidth: width_, pixelHeight: height_, dpiX: 96, dpiY: 96, pixelFormat: PixelFormats.Pbgra32, palette: null);
    var bpp_ = (bitmap_.Format.BitsPerPixel + 7) / 8;
    var stride_ = bitmap_.PixelWidth * bpp_;
    bitmap_.WritePixels(sourceRect: new Int32Rect(x: 0, y: 0, width: width_, height: height_), pixels: pixels_, stride: stride_, offset: 0);
    return bitmap_;
  }
}