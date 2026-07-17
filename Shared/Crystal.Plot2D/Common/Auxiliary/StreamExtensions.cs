using System.IO;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class StreamExtensions {
  public static void CopyTo(this Stream input, Stream output) {
    var buffer_ = new byte[32768];
    while(true) {
      var read_ = input.Read(buffer: buffer_, offset: 0, count: buffer_.Length);
      if(read_ <= 0) {
        return;
      }

      output.Write(buffer: buffer_, offset: 0, count: read_);
    }
  }
}
