using Crystal.Controls.Meters;

namespace Crystal.Shell.Settings;

/// <summary>
/// Pushes the persisted CPU core-strip appearance onto the live dashboard. The core strip binds to
/// <see cref="CoreBarAppearance"/> across the module boundary, so this service mirrors the saved
/// selection there — on construction and whenever the settings are saved — so a change takes effect
/// immediately and is reproduced on the next launch. App-lifetime singleton, resolved eagerly so the
/// saved look is applied before the CPU tile is realized.
/// </summary>
public sealed class GraphAppearanceService {
  private readonly GraphSettingsStore _store;

  public GraphAppearanceService(GraphSettingsStore store) {
    _store = store;
    _store.Changed += ApplyCoreBars;
    ApplyCoreBars();
  }

  // Both this ctor call and store saves originate on the UI thread, so the bound bars update in place.
  private void ApplyCoreBars() {
    var settings = _store.Current;
    CoreBarAppearance.Current.Segmented = settings.CoreBarStyle == CoreBarStyle.SegmentedBar;
    CoreBarAppearance.Current.Monochrome = settings.CoreBarColor == CoreBarColor.Grey;
  }
}
