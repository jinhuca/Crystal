using Crystal.Shell.Settings;
using Crystal.Shell.ViewModels;
using System.Windows.Media;
using Xunit;

namespace Crystal.Shell.Tests;

public class GraphSettingsViewModelTests {
  private static GraphSettingsViewModel NewViewModel() => new(new GraphSettings());

  [Fact]
  public void General_category_applies_to_every_row() {
    var vm = NewViewModel();

    vm.IsFullGraph = true;

    Assert.All(vm.Graphs, r => Assert.Equal(GraphCategory.FullGraph, r.Category));
  }

  [Fact]
  public void General_kind_applies_to_every_row() {
    var vm = NewViewModel();

    vm.IsFilledLine = true;
    Assert.All(vm.Graphs, r => Assert.True(r.IsFilledLine));

    vm.IsSegmentedBar = true;
    Assert.All(vm.Graphs, r => Assert.True(r.IsSegmentedBar));

    vm.IsDot = true;
    Assert.All(vm.Graphs, r => Assert.True(r.IsDot));
  }

  [Fact]
  public void Selecting_dot_deselects_the_other_kinds() {
    var row = NewViewModel().Graphs[0];

    row.IsDot = true;

    Assert.Equal(GraphKindChoice.Dot, row.Kind);
    Assert.True(row.IsDot);
    Assert.False(row.IsFilledLine);
    Assert.False(row.IsSegmentedBar);
  }

  [Fact]
  public void General_history_applies_only_when_in_range() {
    var vm = NewViewModel();

    vm.HistoryLength = 120;
    Assert.All(vm.Graphs, r => Assert.Equal(120, r.HistoryLength));

    // Out-of-range input is kept in the general box but not pushed to the rows.
    vm.HistoryLength = 999;
    Assert.Equal(999, vm.HistoryLength);
    Assert.All(vm.Graphs, r => Assert.Equal(120, r.HistoryLength));

    vm.HistoryLength = 0;
    Assert.All(vm.Graphs, r => Assert.Equal(120, r.HistoryLength));
  }

  [Theory]
  [InlineData(1)]
  [InlineData(240)]
  public void General_history_boundaries_are_valid(int value) {
    var vm = NewViewModel();

    vm.HistoryLength = value;

    Assert.All(vm.Graphs, r => Assert.Equal(value, r.HistoryLength));
  }

  [Fact]
  public void Row_kind_override_does_not_affect_other_rows() {
    var vm = NewViewModel();
    vm.IsFilledLine = true; // level the field first (rows start with mixed defaults)

    vm.Graphs[0].IsSegmentedBar = true;

    Assert.True(vm.Graphs[0].IsSegmentedBar);
    Assert.All(vm.Graphs.Skip(1), r => Assert.True(r.IsFilledLine));
  }

  [Fact]
  public void Reset_restores_defaults_on_every_row_and_the_general_selectors() {
    var vm = NewViewModel();
    vm.IsFullGraph = true;
    vm.IsFilledLine = true;
    vm.HistoryLength = 60;
    vm.Graphs[0].CustomColor = Colors.Red;
    vm.Graphs[1].Accent = GraphAccent.Purple;

    vm.ResetCommand.Execute(null);

    Assert.Equal(GraphCategory.NoFrills, vm.Category);
    Assert.True(vm.IsSegmentedBar);
    Assert.Equal(GraphDefaults.HistoryLength, vm.HistoryLength);
    Assert.All(vm.Graphs, r => {
      Assert.Equal(GraphCategory.NoFrills, r.Category);
      Assert.True(r.IsSegmentedBar);
      Assert.Equal(GraphAccent.Grey, r.Accent);
      Assert.Null(r.CustomColor);
      Assert.False(r.HasCustomColor);
      Assert.Equal(20, r.HistoryLength);
    });
  }

  [Fact]
  public void Setting_custom_color_deselects_every_predefined_swatch() {
    var row = NewViewModel().Graphs[0];

    row.CustomColor = Colors.Red;

    Assert.True(row.HasCustomColor);
    Assert.All(row.AccentOptions, o => Assert.False(o.IsSelected));
  }

  [Fact]
  public void Choosing_a_predefined_accent_clears_the_custom_color() {
    var row = NewViewModel().Graphs[0];
    row.CustomColor = Colors.Red;

    row.Accent = GraphAccent.Sky;

    Assert.Null(row.CustomColor);
    Assert.False(row.HasCustomColor);
    Assert.True(row.AccentOptions.Single(o => o.Accent == GraphAccent.Sky).IsSelected);
  }

  [Fact]
  public void Checking_a_swatch_selects_its_accent_and_clears_the_custom_color() {
    var row = NewViewModel().Graphs[0];
    row.CustomColor = Colors.Red;

    row.AccentOptions.Single(o => o.Accent == GraphAccent.Amber).IsSelected = true;

    Assert.Equal(GraphAccent.Amber, row.Accent);
    Assert.Null(row.CustomColor);
  }

  [Fact]
  public void ToSettings_persists_category_and_custom_color_and_round_trips() {
    var vm = NewViewModel();
    var row = vm.Graphs[0];
    row.IsFullGraph = true;
    row.CustomColor = Color.FromRgb(0x12, 0x34, 0x56);
    row.HistoryLength = 77;

    var settings = vm.ToSettings();
    var saved = settings.Graphs[row.Id];
    Assert.Equal(GraphCategory.FullGraph, saved.Category);
    Assert.Equal("#123456", saved.CustomColor);
    Assert.Equal(77, saved.HistoryLength);

    // Rebuilding from the persisted settings restores the custom colour.
    var reloaded = new GraphSettingsViewModel(settings).Graphs.Single(r => r.Id == row.Id);
    Assert.Equal(Color.FromRgb(0x12, 0x34, 0x56), reloaded.CustomColor);
    Assert.Equal(GraphCategory.FullGraph, reloaded.Category);
  }

  [Fact]
  public void Dot_kind_persists_and_round_trips() {
    var vm = NewViewModel();
    var row = vm.Graphs[0];
    row.IsDot = true;

    var saved = vm.ToSettings().Graphs[row.Id];
    Assert.Equal(GraphKindChoice.Dot, saved.Kind);

    var reloaded = new GraphSettingsViewModel(vm.ToSettings()).Graphs.Single(r => r.Id == row.Id);
    Assert.True(reloaded.IsDot);
  }

  [Fact]
  public void Rows_with_no_custom_color_persist_null() {
    var vm = NewViewModel();

    var saved = vm.ToSettings().Graphs[vm.Graphs[0].Id];

    Assert.Null(saved.CustomColor);
  }

  [Fact]
  public void Core_bar_selectors_seed_from_settings() {
    var vm = new GraphSettingsViewModel(new GraphSettings {
      CoreBarStyle = CoreBarStyle.Bar,
      CoreBarColor = CoreBarColor.Grey,
    });

    Assert.True(vm.IsCoreBar);
    Assert.False(vm.IsCoreSegmentedBar);
    Assert.True(vm.IsCoreGrey);
    Assert.False(vm.IsCoreColorful);
  }

  [Fact]
  public void Core_bar_style_selection_is_mutually_exclusive() {
    var vm = NewViewModel();

    vm.IsCoreBar = true;
    Assert.Equal(CoreBarStyle.Bar, vm.CoreBarStyle);
    Assert.False(vm.IsCoreSegmentedBar);

    vm.IsCoreSegmentedBar = true;
    Assert.Equal(CoreBarStyle.SegmentedBar, vm.CoreBarStyle);
    Assert.False(vm.IsCoreBar);
  }

  [Fact]
  public void Core_bar_settings_persist_and_round_trip() {
    var vm = NewViewModel();
    vm.IsCoreBar = true;
    vm.IsCoreGrey = true;

    var settings = vm.ToSettings();
    Assert.Equal(CoreBarStyle.Bar, settings.CoreBarStyle);
    Assert.Equal(CoreBarColor.Grey, settings.CoreBarColor);

    var reloaded = new GraphSettingsViewModel(settings);
    Assert.True(reloaded.IsCoreBar);
    Assert.True(reloaded.IsCoreGrey);
  }

  [Fact]
  public void Reset_restores_core_bar_defaults() {
    var vm = NewViewModel();
    vm.IsCoreBar = true;
    vm.IsCoreGrey = true;

    vm.ResetCommand.Execute(null);

    Assert.Equal(GraphDefaults.CoreBarStyle, vm.CoreBarStyle);
    Assert.Equal(GraphDefaults.CoreBarColor, vm.CoreBarColor);
    Assert.True(vm.IsCoreSegmentedBar);
    Assert.True(vm.IsCoreColorful);
  }
}
