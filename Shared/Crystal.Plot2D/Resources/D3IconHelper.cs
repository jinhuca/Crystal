using System;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D.Resources;

public static class D3IconHelper {
  private static BitmapFrame _icon;

  public static BitmapFrame TheIcon {
    get {
      if(_icon == null) {
        var currentAssembly_ = typeof(D3IconHelper).Assembly;
        _icon = BitmapFrame.Create(bitmapStream: currentAssembly_.GetManifestResourceStream(name: "Crystal.Plot2D.Resources.D3-icon.ico"));
      }

      return _icon;
    }
  }

  private static BitmapFrame _whiteIcon;

  public static BitmapFrame WhiteIcon {
    get {
      if(_whiteIcon == null) {
        var currentAssembly_ = typeof(D3IconHelper).Assembly;
        _whiteIcon = BitmapFrame.Create(bitmapStream: currentAssembly_.GetManifestResourceStream(name: "Crystal.Plot2D.Resources.D3-icon-white.ico")
                                                     ?? throw new InvalidOperationException());
      }

      return _whiteIcon;
    }
  }
}
