using Crystal.Controls.PerformanceGraphs;
using Crystal.Shell.Settings;
using System.IO;
using Xunit;

namespace Crystal.Shell.Tests;

public class GraphSettingsStoreTests : IDisposable {
  private readonly string _dir =
      Path.Combine(Path.GetTempPath(), "CrystalGraphSettingsTests", Guid.NewGuid().ToString("N"));

  public void Dispose() {
    try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); } catch { }
  }

  [Fact]
  public void Missing_file_yields_defaults() {
    var store = new GraphSettingsStore(_dir);

    Assert.NotNull(store.Current);
    Assert.Equal(GraphRenderMode.Line, store.Current.RenderMode);
    Assert.Equal(GraphDefaults.CoreBarStyle, store.Current.CoreBarStyle);
    Assert.Equal(GraphDefaults.CoreBarColor, store.Current.CoreBarColor);
  }

  [Fact]
  public void Render_mode_round_trips() {
    var store = new GraphSettingsStore(_dir);

    store.Save(new GraphSettings { RenderMode = GraphRenderMode.Dot });

    var reloaded = new GraphSettingsStore(_dir).Current;
    Assert.Equal(GraphRenderMode.Dot, reloaded.RenderMode);
  }

  [Fact]
  public void Core_bar_options_round_trip() {
    var store = new GraphSettingsStore(_dir);

    store.Save(new GraphSettings { CoreBarStyle = CoreBarStyle.Bar, CoreBarColor = CoreBarColor.Grey });

    var reloaded = new GraphSettingsStore(_dir).Current;
    Assert.Equal(CoreBarStyle.Bar, reloaded.CoreBarStyle);
    Assert.Equal(CoreBarColor.Grey, reloaded.CoreBarColor);
  }

  [Fact]
  public void Core_bar_options_default_when_absent_from_file() {
    var store = new GraphSettingsStore(_dir);

    Assert.Equal(GraphDefaults.CoreBarStyle, store.Current.CoreBarStyle);
    Assert.Equal(GraphDefaults.CoreBarColor, store.Current.CoreBarColor);
  }

  [Fact]
  public void Save_raises_changed() {
    var store = new GraphSettingsStore(_dir);
    var raised = false;
    store.Changed += () => raised = true;

    store.Save(new GraphSettings());

    Assert.True(raised);
  }
}
