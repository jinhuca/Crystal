using Crystal.Service.Process;
using Microsoft.Reactive.Testing;
using Crystal.ProcessModule.Models;
using Crystal.ProcessModule.ViewModels;
using Xunit;

namespace Crystal.ProcessModule.Tests;

// Covers the record-to-CSV orchestration on the view model: that starting follows the selected PID,
// each poll appends only that PID's sample, the recording auto-stops when the process exits, and the
// IsRecording / label / CanStartRecording state tracks correctly. The file IO itself is faked.
public class ProcessRecordingTests {
  private static SystemStatsMonitor InertStats() =>
      new(TimeSpan.FromHours(1), new TestScheduler());

  private static ProcessListViewModel CreateVm(out FakeProcessModel model,
                                               out FakeProcessRecorder recorder) {
    model = new FakeProcessModel();
    recorder = new FakeProcessRecorder();
    return new ProcessListViewModel(model, InertStats(), recorder: recorder);
  }

  private static ProcessSample Sample(uint pid, string name, double cpu = 0, double mem = 0) =>
      new(pid, name, cpu, mem, ProcessCategory.BackgroundProcess);

  [Fact]
  public void Start_recording_opens_the_file_for_the_selected_pid() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);

      vm.StartRecording(@"C:\temp\rec.csv");

      Assert.True(vm.IsRecording);
      Assert.True(recorder.IsActive);
      Assert.Equal(@"C:\temp\rec.csv", recorder.FilePath);
      Assert.Equal("Stop rec", vm.RecordButtonLabel);
    });
  }

  [Fact]
  public void Start_recording_is_a_noop_when_nothing_is_selected() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(100, "alpha")]);
      vm.SelectedRow = null;

      vm.StartRecording(@"C:\temp\rec.csv");

      Assert.False(vm.IsRecording);
      Assert.Equal(0, recorder.StartCallCount);
    });
  }

  [Fact]
  public void Each_poll_records_only_the_tracked_pids_sample() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);
      vm.StartRecording(@"C:\temp\rec.csv");

      // Two more polls, each carrying both processes.
      model.Samples.OnNext([Sample(100, "alpha", cpu: 5), Sample(200, "beta", cpu: 10)]);
      model.Samples.OnNext([Sample(100, "alpha", cpu: 6), Sample(200, "beta", cpu: 11)]);

      Assert.Equal(2, recorder.Written.Count);
      Assert.All(recorder.Written, w => Assert.Equal(200u, w.Sample.ProcessId));
      Assert.Equal(10, recorder.Written[0].Sample.CpuPercent);
      Assert.Equal(11, recorder.Written[1].Sample.CpuPercent);
    });
  }

  [Fact]
  public void Recording_follows_the_original_pid_even_after_selection_moves() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);
      vm.StartRecording(@"C:\temp\rec.csv");

      // Move the selection to a different process, then poll.
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 100);
      model.Samples.OnNext([Sample(100, "alpha", cpu: 5), Sample(200, "beta", cpu: 9)]);

      Assert.Single(recorder.Written);
      Assert.Equal(200u, recorder.Written[0].Sample.ProcessId);
    });
  }

  [Fact]
  public void Stop_recording_closes_the_file_and_reports_the_sample_count() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single();
      vm.StartRecording(@"C:\temp\rec.csv");
      model.Samples.OnNext([Sample(200, "beta", cpu: 10)]);

      vm.StopRecording();

      Assert.False(vm.IsRecording);
      Assert.Equal(1, recorder.StopCallCount);
      Assert.Equal("Record", vm.RecordButtonLabel);
      Assert.True(vm.HasActionStatus);
      Assert.Contains("1 sample", vm.ActionStatus);
    });
  }

  [Fact]
  public void Recording_auto_stops_when_the_tracked_process_exits() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);
      vm.StartRecording(@"C:\temp\rec.csv");

      // Next poll: PID 200 is gone.
      model.Samples.OnNext([Sample(100, "alpha")]);

      Assert.False(vm.IsRecording);
      Assert.Equal(1, recorder.StopCallCount);
      Assert.Contains("exited", vm.ActionStatus);
    });
  }

  [Fact]
  public void A_failed_start_surfaces_the_reason_and_does_not_record() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var recorder);
      model.Samples.OnNext([Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single();
      recorder.NextStartResult = ProcessActionResult.Fail("Access to the path is denied.");

      vm.StartRecording(@"C:\protected\rec.csv");

      Assert.False(vm.IsRecording);
      Assert.Equal("Access to the path is denied.", vm.ActionStatus);
      // A later poll must not write anything since the recording never started.
      model.Samples.OnNext([Sample(200, "beta", cpu: 5)]);
      Assert.Empty(recorder.Written);
    });
  }

  [Fact]
  public void Can_start_recording_tracks_selection_and_active_state() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out _);
      model.Samples.OnNext([Sample(200, "beta")]);

      vm.SelectedRow = null;
      Assert.False(vm.CanStartRecording);

      vm.SelectedRow = vm.Rows.Single();
      Assert.True(vm.CanStartRecording);

      vm.StartRecording(@"C:\temp\rec.csv");
      vm.SelectedRow = null; // stays enabled while recording so it can be stopped
      Assert.True(vm.CanStartRecording);
    });
  }

  [Fact]
  public void The_etw_unavailable_reason_is_passed_to_the_recorder() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel { MetricsStatusError = "not elevated" };
      var recorder = new FakeProcessRecorder();
      var vm = new ProcessListViewModel(model, InertStats(), recorder: recorder);
      model.Samples.OnNext([Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single();

      vm.StartRecording(@"C:\temp\rec.csv");

      Assert.Equal("not elevated", recorder.MetricsUnavailablePassed);
    });
  }
}
