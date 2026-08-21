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
    Assert.Equal(GraphCategory.NoFrills, store.Current.Category);
    Assert.Empty(store.Current.Graphs);
  }

  [Fact]
  public void Save_then_reload_round_trips_every_field() {
    var store = new GraphSettingsStore(_dir);
    var settings = new GraphSettings { Category = GraphCategory.FullGraph };
    settings.Graphs["Cpu.Utilization"] = new GraphSetting {
      Category = GraphCategory.FullGraph,
      Kind = GraphKindChoice.FilledLine,
      Accent = GraphAccent.Rose,
      CustomColor = "#ABCDEF",
      HistoryLength = 99,
    };

    store.Save(settings);

    var reloaded = new GraphSettingsStore(_dir).Current;
    Assert.Equal(GraphCategory.FullGraph, reloaded.Category);
    var g = reloaded.Graphs["Cpu.Utilization"];
    Assert.Equal(GraphCategory.FullGraph, g.Category);
    Assert.Equal(GraphKindChoice.FilledLine, g.Kind);
    Assert.Equal(GraphAccent.Rose, g.Accent);
    Assert.Equal("#ABCDEF", g.CustomColor);
    Assert.Equal(99, g.HistoryLength);
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
