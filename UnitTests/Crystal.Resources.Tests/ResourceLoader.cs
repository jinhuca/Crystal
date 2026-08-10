using System.Windows;

namespace Crystal.Resources.Tests;

/// <summary>Loads the shared resource dictionaries by pack URI, exactly as the Shell's App.xaml does.</summary>
internal static class ResourceLoader {
  private static readonly object Gate = new();
  private static bool _initialized;

  static ResourceLoader() {
    // Touching PackUriHelper registers the "pack://" URI *parser*, so Uri construction succeeds.
    _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
  }

  public static ResourceDictionary LoadGeneric() =>
    Load("pack://application:,,,/Crystal.Resources;component/Themes/Generic.xaml");

  public static ResourceDictionary LoadPalette() =>
    Load("pack://application:,,,/Crystal.Resources;component/Styles/Palette.xaml");

  private static ResourceDictionary Load(string packPath) {
    EnsureApplication();
    return new() { Source = new Uri(packPath, UriKind.Absolute) };
  }

  // The pack URI *parser* alone isn't enough: resolving a pack://application:,,, Source goes through
  // WebRequest, whose "pack:" prefix handler is only registered once a System.Windows.Application
  // exists. Without it, ResourceDictionary.Source throws "The URI prefix is not recognized." Creating
  // the single AppDomain-wide Application instance registers that handler. Must run on an STA thread,
  // which callers already provide via StaRunner.
  private static void EnsureApplication() {
    lock (Gate) {
      if (_initialized) return;
      if (System.Windows.Application.Current == null) _ = new System.Windows.Application();
      _initialized = true;
    }
  }
}
