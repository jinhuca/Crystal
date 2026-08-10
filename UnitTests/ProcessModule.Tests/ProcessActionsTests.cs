using Crystal.Service.Process;
using Microsoft.Reactive.Testing;
using ProcessModule.Models;
using ProcessModule.ViewModels;
using Xunit;

namespace ProcessModule.Tests;

// Covers the End task / Run new task actions on the view model: that they route to the injected
// IProcessController with the right arguments, no-op when there's nothing to act on, and surface a
// failure message (clearing it on success).
public class ProcessActionsTests {
  private static SystemStatsMonitor InertStats() =>
      new(TimeSpan.FromHours(1), new TestScheduler());

  private static ProcessListViewModel CreateVm(out FakeProcessModel model,
                                               out FakeProcessController controller) {
    model = new FakeProcessModel();
    controller = new FakeProcessController();
    return new ProcessListViewModel(model, InertStats(), controller: controller);
  }

  private static ProcessSample Sample(uint pid, string name) =>
      new(pid, name, 0, 0, ProcessCategory.BackgroundProcess);

  [Fact]
  public void End_selected_task_kills_the_selected_pid() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var controller);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 200);

      vm.EndSelectedTask();

      Assert.Equal(1, controller.EndCallCount);
      Assert.Equal(200u, controller.EndedPid);
    });
  }

  [Fact]
  public void End_selected_task_is_a_noop_when_nothing_is_selected() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var controller);
      model.Samples.OnNext([Sample(100, "alpha")]);
      vm.SelectedRow = null;

      vm.EndSelectedTask();

      Assert.Equal(0, controller.EndCallCount);
    });
  }

  [Fact]
  public void Can_end_selected_task_tracks_the_selection() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out _);
      model.Samples.OnNext([Sample(100, "alpha")]);

      vm.SelectedRow = vm.Rows.Single();
      Assert.True(vm.CanEndSelectedTask);

      vm.SelectedRow = null;
      Assert.False(vm.CanEndSelectedTask);
    });
  }

  [Fact]
  public void A_failed_end_task_surfaces_the_reason_as_status() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var controller);
      model.Samples.OnNext([Sample(100, "alpha")]);
      vm.SelectedRow = vm.Rows.Single();
      controller.NextResult = ProcessActionResult.Fail("Access is denied.");

      vm.EndSelectedTask();

      Assert.True(vm.HasActionStatus);
      Assert.Equal("Access is denied.", vm.ActionStatus);
    });
  }

  [Fact]
  public void A_successful_end_task_clears_any_prior_status() {
    StaRunner.Run(() => {
      var vm = CreateVm(out var model, out var controller);
      model.Samples.OnNext([Sample(100, "alpha")]);
      vm.SelectedRow = vm.Rows.Single();

      controller.NextResult = ProcessActionResult.Fail("boom");
      vm.EndSelectedTask();
      Assert.True(vm.HasActionStatus);

      controller.NextResult = ProcessActionResult.Ok;
      vm.EndSelectedTask();

      Assert.False(vm.HasActionStatus);
      Assert.Null(vm.ActionStatus);
    });
  }

  [Fact]
  public void Start_task_launches_the_command() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _, out var controller);

      vm.StartTask("notepad", runAsAdmin: false);

      Assert.Equal(1, controller.StartCallCount);
      Assert.Equal("notepad", controller.StartedCommand);
      Assert.False(controller.StartedAsAdmin);
    });
  }

  [Fact]
  public void Start_task_forwards_the_admin_flag() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _, out var controller);

      vm.StartTask("cmd", runAsAdmin: true);

      Assert.True(controller.StartedAsAdmin);
    });
  }

  [Fact]
  public void A_failed_start_task_surfaces_the_reason_as_status() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _, out var controller);
      controller.NextResult = ProcessActionResult.Fail("Couldn't run \"bogus\": not found");

      vm.StartTask("bogus");

      Assert.True(vm.HasActionStatus);
      Assert.Equal("Couldn't run \"bogus\": not found", vm.ActionStatus);
    });
  }

  [Fact]
  public void Open_file_location_forwards_the_image_path() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _, out var controller);

      vm.OpenFileLocation(@"C:\Windows\explorer.exe");

      Assert.Equal(1, controller.OpenLocationCallCount);
      Assert.Equal(@"C:\Windows\explorer.exe", controller.OpenedLocationPath);
    });
  }

  [Fact]
  public void A_failed_open_file_location_surfaces_the_reason_as_status() {
    StaRunner.Run(() => {
      var vm = CreateVm(out _, out var controller);
      controller.NextResult = ProcessActionResult.Fail("File location is unavailable for this process.");

      vm.OpenFileLocation(null);

      Assert.True(vm.HasActionStatus);
      Assert.Equal("File location is unavailable for this process.", vm.ActionStatus);
    });
  }
}
