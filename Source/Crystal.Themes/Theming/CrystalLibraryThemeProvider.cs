using ControlzEx.Theming;

namespace Crystal.Themes.Theming
{
  /// <summary>
  /// Provides theme resources from Crystal.Themes.
  /// </summary>
  public class CrystalLibraryThemeProvider : LibraryThemeProvider
  {
    public static readonly CrystalLibraryThemeProvider DefaultInstance = new CrystalLibraryThemeProvider();

    /// <inheritdoc cref="LibraryThemeProvider" />
    public CrystalLibraryThemeProvider()
        : base(true)
    {
    }

    public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
    {
      values.Add("Crystal.Colors.AccentBase", colorValues.AccentBaseColor.ToString());
      values.Add("Crystal.Colors.Accent", colorValues.AccentColor80.ToString());
      values.Add("Crystal.Colors.Accent2", colorValues.AccentColor60.ToString());
      values.Add("Crystal.Colors.Accent3", colorValues.AccentColor40.ToString());
      values.Add("Crystal.Colors.Accent4", colorValues.AccentColor20.ToString());

      values.Add("Crystal.Colors.Highlight", colorValues.HighlightColor.ToString());
      values.Add("Crystal.Colors.IdealForeground", colorValues.IdealForegroundColor.ToString());
    }
  }
}
