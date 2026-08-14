using Crystal.ProcessModule.ViewModels;
using Crystal.Service.Process;
using Microsoft.Reactive.Testing;
using System.Linq;
using Xunit;

namespace Crystal.ProcessModule.Tests;

public class ProcessListViewModelTests {
  // A stats monitor wired to a never-advanced TestScheduler: its poll timer never ticks, so the VM
  // never calls the real Process.GetProcesses() table. Keeps these tests to pushed samples only.
  private static SystemStatsMonitor InertStats() =>
      new(TimeSpan.FromHours(1), new TestScheduler());

  private static ProcessListViewModel CreateVm(out FakeProcessModel model) {
    model = new FakeProcessModel();
    return new ProcessListViewModel(model, InertStats());
  }

  private static ProcessListViewModel CreateVm(out FakeProcessModel model, Func<DateTimeOffset> clock) {
    model = new FakeProcessModel();
    return new ProcessListViewModel(model, InertStats(), clock: clock);
  }

  private static ProcessSample Sample(
      uint pid, string name, double cpu = 0, double mem = 0,
      ProcessCategory category = ProcessCategory.BackgroundProcess) =>
      new(pid, name, cpu, mem, category);

  private static string[] ExportLines(ProcessListViewModel vm) =>
      vm.RowsAsText()
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.TrimEnd('\r'))
        .ToArray();

  [Fact]
  public void A_new_pid_adds_a_row() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);

      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);

      Assert.Equal(2, vm.Rows.Count);
      Assert.Contains(vm.Rows, r => r.ProcessId == 100 && r.Name == "alpha");
      Assert.Contains(vm.Rows, r => r.ProcessId == 200 && r.Name == "beta");
    });
  }

  [Fact]
  public void An_existing_pid_updates_in_place_without_replacing_the_row() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);

      model.Samples.OnNext([Sample(100, "alpha", cpu: 10)]);
      var original = vm.Rows.Single();

      model.Samples.OnNext([Sample(100, "alpha", cpu: 55, mem: 128)]);

      Assert.Single(vm.Rows);
      Assert.Same(original, vm.Rows.Single());   // same instance → selection/bindings survive
      Assert.Equal(55, vm.Rows.Single().CpuPercent);
      Assert.Equal(128, vm.Rows.Single().WorkingSetMb);
    });
  }

  [Fact]
  public void An_exited_pid_is_dropped() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);

      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      model.Samples.OnNext([Sample(100, "alpha")]);   // 200 gone this poll

      Assert.Single(vm.Rows);
      Assert.Equal(100u, vm.Rows.Single().ProcessId);
    });
  }

  [Fact]
  public void Dropping_the_selected_row_clears_the_selection() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);

      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);

      model.Samples.OnNext([Sample(100, "alpha")]);   // selected 200 exits

      Assert.Null(vm.SelectedRow);
    });
  }

  [Fact]
  public void The_own_process_row_is_selected_by_default_once_it_appears() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      uint ownPid = (uint)Environment.ProcessId;

      model.Samples.OnNext([Sample(100, "alpha")]);
      Assert.Null(vm.SelectedRow);                    // own row not present yet

      model.Samples.OnNext([Sample(100, "alpha"), Sample(ownPid, "self")]);

      Assert.NotNull(vm.SelectedRow);
      Assert.Equal(ownPid, vm.SelectedRow!.ProcessId);
    });
  }

  [Fact]
  public void The_default_selection_is_applied_only_once() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      uint ownPid = (uint)Environment.ProcessId;

      model.Samples.OnNext([Sample(ownPid, "self")]);
      var chosen = vm.SelectedRow;
      vm.SelectedRow = null;                          // user deselects

      model.Samples.OnNext([Sample(ownPid, "self")]); // another poll must not re-grab the selection

      Assert.NotNull(chosen);
      Assert.Null(vm.SelectedRow);
    });
  }

  [Fact]
  public void Cpu_descending_is_the_default_sort() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _);

      Assert.Equal(nameof(ProcessRowViewModel.CpuPercent), vm.SortProperty);
      Assert.Equal(System.ComponentModel.ListSortDirection.Descending, vm.SortDirection);
    });
  }

  [Fact]
  public void Clicking_a_new_column_sorts_it_ascending() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _);

      vm.SortBy(nameof(ProcessRowViewModel.Name));

      Assert.Equal(nameof(ProcessRowViewModel.Name), vm.SortProperty);
      Assert.Equal(System.ComponentModel.ListSortDirection.Ascending, vm.SortDirection);
    });
  }

  [Fact]
  public void Clicking_the_active_column_flips_the_direction() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _);
      vm.SortBy(nameof(ProcessRowViewModel.Name));    // now Name ascending

      vm.SortBy(nameof(ProcessRowViewModel.Name));    // repeat → descending
      Assert.Equal(System.ComponentModel.ListSortDirection.Descending, vm.SortDirection);

      vm.SortBy(nameof(ProcessRowViewModel.Name));    // repeat again → ascending
      Assert.Equal(System.ComponentModel.ListSortDirection.Ascending, vm.SortDirection);
    });
  }

  [Fact]
  public void Name_filter_is_a_case_insensitive_substring_match() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(1, "chrome"), Sample(2, "explorer"), Sample(3, "Code")]);

      vm.NameFilter = "co";   // matches "Code" (case-insensitive), not chrome/explorer

      var shown = vm.RowsView.Cast<ProcessRowViewModel>().ToList();
      Assert.Single(shown);
      Assert.Equal("Code", shown[0].Name);
    });
  }

  [Fact]
  public void Clearing_the_name_filter_restores_all_rows() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(1, "chrome"), Sample(2, "explorer")]);
      vm.NameFilter = "chrome";
      Assert.Single(vm.RowsView.Cast<ProcessRowViewModel>());

      vm.NameFilter = "";

      Assert.Equal(2, vm.RowsView.Cast<ProcessRowViewModel>().Count());
    });
  }

  [Fact]
  public void Pid_filter_matches_the_decimal_text_of_the_pid() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(1234, "a"), Sample(5678, "b"), Sample(2340, "c")]);

      vm.PidFilter = "234";   // substring of 1234 and 2340, not 5678

      var shown = vm.RowsView.Cast<ProcessRowViewModel>().Select(r => r.ProcessId).ToHashSet();
      Assert.Equal([1234u, 2340u], shown.OrderBy(x => x));
    });
  }

  [Fact]
  public void Name_and_pid_filters_combine_as_an_and() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(100, "chrome"), Sample(1001, "chrome"), Sample(1001, "edge")]);

      vm.NameFilter = "chrome";
      vm.PidFilter = "100";

      var shown = vm.RowsView.Cast<ProcessRowViewModel>().ToList();
      Assert.All(shown, r => Assert.Contains("chrome", r.Name, StringComparison.OrdinalIgnoreCase));
      Assert.All(shown, r => Assert.Contains("100", r.ProcessId.ToString()));
    });
  }

  [Fact]
  public void Metrics_status_error_surfaces_from_the_model() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel { MetricsStatusError = "not elevated" };
      var vm = new ProcessListViewModel(model, InertStats());

      Assert.True(vm.HasMetricsStatusError);
      Assert.Equal("not elevated", vm.MetricsStatusError);
    });
  }

  [Fact]
  public void No_metrics_status_error_when_the_model_reports_none() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _);

      Assert.False(vm.HasMetricsStatusError);
      Assert.Null(vm.MetricsStatusError);
    });
  }

  [Fact]
  public void Has_visible_rows_is_false_until_a_row_appears_and_tracks_the_filter() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      Assert.False(vm.HasVisibleRows);

      model.Samples.OnNext([Sample(1, "chrome"), Sample(2, "edge")]);
      Assert.True(vm.HasVisibleRows);

      vm.NameFilter = "nothing-matches-this";
      Assert.False(vm.HasVisibleRows);   // filter hides every row

      vm.NameFilter = "";
      Assert.True(vm.HasVisibleRows);
    });
  }

  [Fact]
  public void Hog_count_reflects_rows_that_crossed_a_peak_threshold() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      Assert.Equal(0, vm.HogCount);

      model.Samples.OnNext([
          Sample(1, "chrome", cpu: 80, mem: 100),     // CPU hog
          Sample(2, "edge", cpu: 5, mem: 2048),        // memory hog
          Sample(3, "idle", cpu: 2, mem: 50),          // neither
      ]);

      Assert.Equal(2, vm.HogCount);
    });
  }

  [Fact]
  public void Hog_count_holds_after_the_live_reading_dips_then_clears_on_reset() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(1, "chrome", cpu: 80, mem: 2048)]);
      model.Samples.OnNext([Sample(1, "chrome", cpu: 1, mem: 50)]);   // live dips, peak holds

      Assert.Equal(1, vm.HogCount);

      vm.ResetAllPeaks();

      Assert.Equal(0, vm.HogCount);
    });
  }

  [Fact]
  public void Hog_count_ignores_the_active_filter() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([
          Sample(1, "chrome", cpu: 80, mem: 100),   // hog
          Sample(2, "edge", cpu: 90, mem: 100),     // hog
      ]);
      Assert.Equal(2, vm.HogCount);

      vm.NameFilter = "chrome";   // hides edge from the view but it's still a session hog

      Assert.Equal(2, vm.HogCount);
    });
  }

  [Fact]
  public void Reset_all_peaks_collapses_every_rows_peaks_to_current() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model);
      model.Samples.OnNext([Sample(1, "chrome", cpu: 80, mem: 800), Sample(2, "edge", cpu: 60, mem: 400)]);
      model.Samples.OnNext([Sample(1, "chrome", cpu: 5, mem: 100), Sample(2, "edge", cpu: 2, mem: 50)]);

      vm.ResetAllPeaks();

      foreach (var row in vm.Rows) {
        Assert.Equal(row.CpuPercent, row.PeakCpuPercent);
        Assert.Equal(row.WorkingSetMb, row.PeakWorkingSetMb);
      }
    });
  }

  [Fact]
  public void Export_is_empty_when_no_rows_are_visible() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _);
      Assert.Equal("", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_leads_with_the_capture_timestamp_and_a_count() {
    StaRunner.Run(() => {
      var now = new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero);
      var vm = CreateVm(out var model, () => now);

      model.Samples.OnNext([Sample(1, "chrome"), Sample(2, "edge")]);

      var lines = vm.RowsAsText()
          .Split('\n', StringSplitOptions.RemoveEmptyEntries)
          .Select(l => l.TrimEnd('\r'))
          .ToArray();

      Assert.Equal($"# Exported {now.LocalDateTime:yyyy-MM-dd HH:mm:ss}", lines[0]);
      Assert.Equal("# 2 process(es)", lines[1]);
      Assert.Equal("# 2 background", lines[2]);   // both default to BackgroundProcess
      Assert.Equal("# Sorted by category, then CPU descending", lines[3]);
      Assert.Equal("Group\tName\tPID\tStatus\tCPU%\tCPU pk%\tGPU%\tMemory MB\tMem pk MB\tDisk B/s\tNet B/s", lines[4]);
    });
  }

  [Fact]
  public void Export_row_carries_the_live_and_peak_values_with_placeholders_for_null_metrics() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));

      model.Samples.OnNext([Sample(1234, "chrome", cpu: 40, mem: 512)]);  // GPU/Disk/Net null
      model.Samples.OnNext([Sample(1234, "chrome", cpu: 5, mem: 128)]);   // dip → peaks hold

      var lines = vm.RowsAsText()
          .Split('\n', StringSplitOptions.RemoveEmptyEntries)
          .Select(l => l.TrimEnd('\r'))
          .ToArray();
      var cols = lines[5].Split('\t');   // [0]stamp [1]count [2]group [3]sort [4]header [5]row

      Assert.Equal("chrome", cols[1]);
      Assert.Equal("1234", cols[2]);
      Assert.Equal("5.0", cols[4]);    // live CPU dipped
      Assert.Equal("40.0", cols[5]);   // CPU peak held
      Assert.Equal("-", cols[6]);      // GPU null → placeholder
      Assert.Equal("128", cols[7]);    // live memory
      Assert.Equal("512", cols[8]);    // memory peak held
      Assert.Equal("-", cols[9]);      // disk null
      Assert.Equal("-", cols[10]);     // net null
    });
  }

  [Fact]
  public void Filtered_export_is_flagged_and_carries_only_the_shown_rows() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome"), Sample(2, "edge")]);

      vm.NameFilter = "chrome";

      var text = vm.RowsAsText();
      var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
          .Select(l => l.TrimEnd('\r'))
          .ToArray();

      Assert.Equal("# 1 process(es)", lines[1]);
      Assert.Equal("# 1 background", lines[2]);   // category breakdown of the shown row
      Assert.Equal("# Sorted by category, then CPU descending", lines[3]);
      Assert.Equal("# Filtered view: only rows matching the active name/PID filter", lines[4]);
      Assert.Contains("chrome", text);
      Assert.DoesNotContain("edge", text);
    });
  }

  [Fact]
  public void Export_states_the_default_sort_order() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome")]);

      Assert.Contains("# Sorted by category, then CPU descending", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_reflects_a_chosen_sort_column_and_direction() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome")]);

      vm.SortBy(nameof(ProcessRowViewModel.Name));   // new column → ascending

      Assert.Contains("# Sorted by category, then Name ascending", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_breaks_down_the_visible_rows_by_category() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([
          Sample(1, "chrome", category: ProcessCategory.App),
          Sample(2, "notepad", category: ProcessCategory.App),
          Sample(3, "svc", category: ProcessCategory.BackgroundProcess),
          Sample(4, "csrss", category: ProcessCategory.WindowsProcess),
      ]);

      var lines = ExportLines(vm);

      Assert.Equal("# 4 process(es)", lines[1]);
      Assert.Equal("# 2 app(s), 1 background, 1 windows", lines[2]);
    });
  }

  [Fact]
  public void Export_category_breakdown_omits_absent_categories() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([
          Sample(1, "svc", category: ProcessCategory.BackgroundProcess),
          Sample(2, "svc2", category: ProcessCategory.BackgroundProcess),
      ]);

      var lines = ExportLines(vm);

      Assert.Equal("# 2 background", lines[2]);   // no app/windows clauses
    });
  }

  [Fact]
  public void Export_category_breakdown_counts_only_the_visible_rows() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([
          Sample(1, "chrome", category: ProcessCategory.App),
          Sample(2, "svc", category: ProcessCategory.BackgroundProcess),
      ]);

      vm.NameFilter = "chrome";   // hides the background row

      var lines = ExportLines(vm);
      Assert.Equal("# 1 process(es)", lines[1]);
      Assert.Equal("# 1 app(s)", lines[2]);   // background row excluded from the breakdown
    });
  }

  [Fact]
  public void Export_notes_the_session_hog_count_when_any_process_spiked() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([
          Sample(1, "chrome", cpu: 80, mem: 100),   // CPU hog
          Sample(2, "edge", cpu: 5, mem: 2048),      // memory hog
          Sample(3, "idle", cpu: 1, mem: 50),        // neither
      ]);

      var text = vm.RowsAsText();

      Assert.Contains("# 2 sustained hog(s):", text);
    });
  }

  [Fact]
  public void Export_hog_note_reports_the_session_count_ignoring_the_filter() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([
          Sample(1, "chrome", cpu: 80, mem: 100),
          Sample(2, "edge", cpu: 90, mem: 100),
      ]);

      vm.NameFilter = "chrome";   // hides edge from the export rows, but it's still a session hog

      var text = vm.RowsAsText();

      Assert.Contains("# 2 sustained hog(s):", text);   // counts both, not just the shown row
    });
  }

  [Fact]
  public void Export_omits_the_hog_note_when_nothing_spiked() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "idle", cpu: 1, mem: 50)]);

      Assert.DoesNotContain("sustained hog", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_notes_when_gpu_disk_network_metrics_are_unavailable() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel { MetricsStatusError = "not elevated" };
      var vm = new ProcessListViewModel(model, InertStats(),
          clock: () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome", cpu: 10, mem: 100)]);

      var text = vm.RowsAsText();

      Assert.Contains("# GPU/Disk/Network unavailable (not elevated) — shown as '-'", text);
    });
  }

  [Fact]
  public void Export_omits_the_metrics_note_when_metrics_are_live() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome", cpu: 10, mem: 100)]);

      Assert.DoesNotContain("GPU/Disk/Network unavailable", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_omits_the_reset_note_until_peaks_are_reset() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome", cpu: 80, mem: 100)]);

      Assert.DoesNotContain("Peaks were reset", vm.RowsAsText());
    });
  }

  [Fact]
  public void Export_notes_when_peaks_were_reset_this_session() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome", cpu: 80, mem: 100)]);

      vm.ResetAllPeaks();

      var lines = vm.RowsAsText()
          .Split('\n', StringSplitOptions.RemoveEmptyEntries)
          .Select(l => l.TrimEnd('\r'))
          .ToArray();

      Assert.Equal("# 1 process(es)", lines[1]);
      Assert.Equal("# 1 background", lines[2]);   // category breakdown
      Assert.Equal("# Sorted by category, then CPU descending", lines[3]);
      Assert.StartsWith("# Peaks were reset this session", lines[4]);   // after count + breakdown + sort, before any filter note
    });
  }

  [Fact]
  public void Unfiltered_export_omits_the_filter_note() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, () => new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero));
      model.Samples.OnNext([Sample(1, "chrome")]);

      Assert.DoesNotContain("Filtered view", vm.RowsAsText());
    });
  }
}
