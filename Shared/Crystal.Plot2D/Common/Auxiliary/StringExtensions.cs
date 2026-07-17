namespace Crystal.Plot2D.Common.Auxiliary;

internal static class StringExtensions {
  public static string Format(this string formatString, object param) {
    return string.Format(format: formatString, arg0: param);
  }

  public static string Format(this string formatString, object param1, object param2) {
    return string.Format(format: formatString, arg0: param1, arg1: param2);
  }
}
