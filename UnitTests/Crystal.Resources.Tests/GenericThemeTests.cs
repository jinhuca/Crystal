using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Crystal.Resources.Tests;

/// <summary>
/// Smoke test for Themes/Generic.xaml, the composite dictionary the Shell actually merges. It pulls
/// in ~10 style dictionaries; a broken Source path or a XAML parse error in any of them surfaces
/// here as a load failure instead of at app startup. Also confirms the palette brushes are reachable
/// through the merge chain, since the shared styles depend on them being in scope.
/// </summary>
public class GenericThemeTests {
  [Fact]
  public void Generic_LoadsWithoutError() =>
    StaRunner.Run(() => {
      var generic = ResourceLoader.LoadGeneric();
      Assert.NotEmpty(generic.MergedDictionaries);
    });

  [Theory]
  [MemberData(nameof(PaletteTests.ExpectedData), MemberType = typeof(PaletteTests))]
  public void Generic_ExposesPaletteBrush(string key, string hex) =>
    StaRunner.Run(() => {
      var generic = ResourceLoader.LoadGeneric();

      Assert.True(generic.Contains(key), $"Generic.xaml does not expose palette key '{key}' through its merge chain.");
      var brush = Assert.IsType<SolidColorBrush>(generic[key]);
      Assert.Equal((Color)ColorConverter.ConvertFromString(hex)!, brush.Color);
    });
}
