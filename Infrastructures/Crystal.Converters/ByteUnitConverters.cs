namespace Crystal.Converters;

public class ByteUnitConverters {
  public static string ConvertBytesToReadableUnit(ulong bytes) {
    string[] units = { "B", "KB", "MB", "GB", "TB" };
    double size = bytes;
    int unitIndex = 0;

    if (bytes < 0) {
      throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes cannot be negative.");
    }

    if (bytes < 1024) {
      return $"{bytes} B";
    }

    while (size >= 1024 && unitIndex < units.Length - 1) {
      size /= 1024;
      unitIndex++;
    }

    return $"{size:F2} {units[unitIndex]}";
  }

  public static long ConvertReadableUnitToBytes(string sizeString) {
    var parts = sizeString.Trim().Split(' ');
    if (parts.Length < 2) return (long)double.Parse(parts[0]);

    double value = double.Parse(parts[0]);
    string unit = parts[1].ToUpper();

    // Map units to their power of 1024
    int power = unit switch {
      "KB" => 1,
      "MB" => 2,
      "GB" => 3,
      "TB" => 4,
      "PB" => 5,
      _ => 0
    };

    return (long)(value * Math.Pow(1024, power));
  }
}
