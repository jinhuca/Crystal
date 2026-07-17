using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class ScreenshotHelper {
  /// <summary>
  ///   Gets the encoder by extension.
  /// </summary>
  /// <param name="extension">The extension</param>
  /// <returns>BitmapEncoder object</returns>
  internal static BitmapEncoder GetEncoderByExtension(string extension) {
    switch(extension) {
      case "bmp":
        return new BmpBitmapEncoder();
      case "jpg":
        return new JpegBitmapEncoder();
      case "gif":
        return new GifBitmapEncoder();
      case "png":
        return new PngBitmapEncoder();
      case "tiff":
        return new TiffBitmapEncoder();
      case "wmp":
        return new WmpBitmapEncoder();
      default:
        throw new ArgumentException(message: Strings.Exceptions.CannotDetermineImageTypeByExtension, paramName: nameof(extension));
    }
  }

  /// <summary>
  /// Creates the screenshot of entire plotter element
  /// </summary>
  /// <returns></returns>
  internal static BitmapSource CreateScreenshot(UIElement uiElement, Int32Rect screenshotSource) {
    var window = Window.GetWindow(dependencyObject: uiElement);
    if(window == null) {
      return CreateElementScreenshot(uiElement: uiElement);
    }
    var size = window.RenderSize;

    //double dpiCoeff = 32 / SystemParameters.CursorWidth;
    //int dpi = (int)(dpiCoeff * 96);
    double dpiCoeff = 1;
    var dpi = 96;

    RenderTargetBitmap bmp = new(pixelWidth: (int)(size.Width * dpiCoeff), pixelHeight: (int)(size.Height * dpiCoeff), dpiX: dpi, dpiY: dpi, pixelFormat: PixelFormats.Default);

    // white background
    Rectangle whiteRect = new() { Width = size.Width, Height = size.Height, Fill = Brushes.White };
    whiteRect.Measure(availableSize: size);
    whiteRect.Arrange(finalRect: new Rect(size: size));
    bmp.Render(visual: whiteRect);
    // the very element
    bmp.Render(visual: uiElement);

    CroppedBitmap croppedBmp = new(source: bmp, sourceRect: screenshotSource);
    return croppedBmp;
  }

  private static BitmapSource CreateElementScreenshot(UIElement uiElement) {
    var measureValid = uiElement.IsMeasureValid;

    if(!measureValid) {
      double width = 300;
      double height = 300;

      if(uiElement is FrameworkElement frElement) {
        if(!double.IsNaN(d: frElement.Width)) {
          width = frElement.Width;
        }
        if(!double.IsNaN(d: frElement.Height)) {
          height = frElement.Height;
        }
      }

      Size size = new(width: width, height: height);
      uiElement.Measure(availableSize: size);
      uiElement.Arrange(finalRect: new Rect(size: size));
    }

    RenderTargetBitmap bmp = new(
      pixelWidth: (int)uiElement.RenderSize.Width,
      pixelHeight: (int)uiElement.RenderSize.Height,
      dpiX: 96,
      dpiY: 96,
      pixelFormat: PixelFormats.Default);

    // this is waiting for dispatcher to perform measure, arrange and render passes
    uiElement.Dispatcher.Invoke(callback: (Action)(() => { }), priority: DispatcherPriority.Background);

    var elementSize = uiElement.DesiredSize;
    // white background
    Rectangle whiteRect = new() { Width = elementSize.Width, Height = elementSize.Height, Fill = Brushes.White };
    whiteRect.Measure(availableSize: elementSize);
    whiteRect.Arrange(finalRect: new Rect(size: elementSize));
    bmp.Render(visual: whiteRect);
    bmp.Render(visual: uiElement);
    return bmp;
  }

  private static readonly Dictionary<BitmapSource, string> pendingBitmaps = new();

  internal static void SaveBitmapToStream(BitmapSource bitmap, Stream stream, string fileExtension) {
    ArgumentNullException.ThrowIfNull(bitmap);
    ArgumentNullException.ThrowIfNull(stream);
    if(string.IsNullOrEmpty(value: fileExtension)) {
      throw new ArgumentException(message: Strings.Exceptions.ExtensionCannotBeNullOrEmpty, paramName: fileExtension);
    }
    var encoder_ = GetEncoderByExtension(extension: fileExtension);
    encoder_.Frames.Add(item: BitmapFrame.Create(
      source: bitmap,
      thumbnail: null,
      metadata: new BitmapMetadata(containerFormat: fileExtension.Trim(trimChar: '.')),
      colorContexts: null));
    encoder_.Save(stream: stream);
  }

  internal static void SaveBitmapToFile(BitmapSource bitmap, string filePath) {
    if(string.IsNullOrEmpty(value: filePath)) {
      throw new ArgumentException(message: Strings.Exceptions.FilePathCannotbeNullOrEmpty, paramName: nameof(filePath));
    }

    if(bitmap.IsDownloading) {
      pendingBitmaps[key: bitmap] = filePath;
      bitmap.DownloadCompleted += OnBitmapDownloadCompleted;
      return;
    }

    var dirPath = System.IO.Path.GetDirectoryName(path: filePath);
    if(!string.IsNullOrEmpty(value: dirPath) && !Directory.Exists(path: dirPath)) {
      Directory.CreateDirectory(path: dirPath);
    }

    var fileExistedBefore_ = File.Exists(path: filePath);
    try {
      using FileStream fs = new(path: filePath, mode: FileMode.Create, access: FileAccess.Write);
      var extension_ = System.IO.Path.GetExtension(path: filePath).TrimStart(trimChar: '.');
      SaveBitmapToStream(bitmap: bitmap, stream: fs, fileExtension: extension_);
    }
    catch(ArgumentException) {
      if(!fileExistedBefore_ && File.Exists(path: filePath)) {
        try {
          File.Delete(path: filePath);
        }
        catch { }
      }
    }
    catch(IOException exc) {
      Debug.WriteLine(message: "Exception while saving bitmap to file: " + exc.Message);
    }
  }

  public static void SaveStreamToFile(Stream stream, string filePath) {
    var dirPath = System.IO.Path.GetDirectoryName(path: filePath);
    if(!string.IsNullOrEmpty(value: dirPath) && !Directory.Exists(path: dirPath)) {
      Directory.CreateDirectory(path: dirPath);
    }

    using(FileStream fs = new(path: filePath, mode: FileMode.Create, access: FileAccess.Write)) {
      var extension = System.IO.Path.GetExtension(path: filePath).TrimStart(trimChar: '.');
      if(stream.CanSeek) {
        stream.Seek(offset: 0, origin: SeekOrigin.Begin);
      }
      stream.CopyTo(destination: fs);
    }

    stream.Dispose();
  }

  private static void OnBitmapDownloadCompleted(object sender, EventArgs e) {
    var bmp = (BitmapSource)sender;
    bmp.DownloadCompleted -= OnBitmapDownloadCompleted;
    var filePath = pendingBitmaps[key: bmp];
    pendingBitmaps.Remove(key: bmp);
    SaveBitmapToFile(bitmap: bmp, filePath: filePath);
  }
}
