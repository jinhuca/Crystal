using ThreadState = System.Diagnostics.ThreadState;
using System.Diagnostics;
using Crystal.ProcessModule.Models;
using Crystal.ProcessModule.ViewModels;
using Crystal.Service.Process;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.ProcessModule.Tests;

// Covers the expandable owned-thread list on the process view model: expanding a row pulls its
// threads from the injected provider, they reconcile in place each poll (keyed by TID), and
// collapsing releases them. Also covers ending a specific (context-menu) row rather than the
// selection.
public class ProcessThreadListTests {
  private static SystemStatsMonitor InertStats() =>
      new(TimeSpan.FromHours(1), new TestScheduler());

  private static ProcessSample Sample(uint pid, string name) =>
      new(pid, name, 0, 0, ProcessCategory.BackgroundProcess);

  private static ThreadSample Thread(int tid, ThreadState state = ThreadState.Running) =>
      new(tid, TimeSpan.Zero, state, null, 8, 8, null);

  [Fact]
  public void Expanding_a_row_populates_its_threads_from_the_provider() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel();
      var threads = new FakeProcessThreadProvider();
      threads.Threads[100] = [Thread(1), Thread(2), Thread(3)];
      var vm = new ProcessListViewModel(model, InertStats(), threadProvider: threads);
      model.Samples.OnNext([Sample(100, "alpha")]);
      var row = vm.Rows.Single();

      row.IsExpanded = true;

      Assert.Equal(new[] { 1, 2, 3 }, row.Threads.Select(t => t.ThreadId));
    });
  }

  [Fact]
  public void Collapsing_a_row_clears_its_threads() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel();
      var threads = new FakeProcessThreadProvider();
      threads.Threads[100] = [Thread(1), Thread(2)];
      var vm = new ProcessListViewModel(model, InertStats(), threadProvider: threads);
      model.Samples.OnNext([Sample(100, "alpha")]);
      var row = vm.Rows.Single();
      row.IsExpanded = true;
      Assert.NotEmpty(row.Threads);

      row.IsExpanded = false;

      Assert.Empty(row.Threads);
    });
  }

  [Fact]
  public void An_expanded_row_reconciles_threads_in_place_each_poll() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel();
      var threads = new FakeProcessThreadProvider();
      threads.Threads[100] = [Thread(1), Thread(2)];
      var vm = new ProcessListViewModel(model, InertStats(), threadProvider: threads);
      model.Samples.OnNext([Sample(100, "alpha")]);
      var row = vm.Rows.Single();
      row.IsExpanded = true;
      var thread1 = row.Threads.Single(t => t.ThreadId == 1);

      // Thread 2 exits, thread 3 appears; thread 1 stays and gets a fresh CPU reading.
      threads.Threads[100] = [
          new ThreadSample(1, TimeSpan.FromSeconds(5), ThreadState.Running, null, 8, 9, null),
          Thread(3),
      ];
      model.Samples.OnNext([Sample(100, "alpha")]);

      Assert.Equal(new[] { 1, 3 }, row.Threads.Select(t => t.ThreadId).OrderBy(id => id));
      Assert.Same(thread1, row.Threads.Single(t => t.ThreadId == 1)); // same instance, updated in place
      Assert.Equal(TimeSpan.FromSeconds(5), thread1.TotalProcessorTime);
    });
  }

  [Fact]
  public void A_collapsed_row_is_not_polled_for_threads() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel();
      var threads = new FakeProcessThreadProvider();
      var vm = new ProcessListViewModel(model, InertStats(), threadProvider: threads);
      model.Samples.OnNext([Sample(100, "alpha")]);

      model.Samples.OnNext([Sample(100, "alpha")]); // another poll, still collapsed

      Assert.Empty(threads.Calls);
    });
  }

  [Fact]
  public void Wait_state_label_appends_the_wait_reason() {
    var thread = new ThreadRowViewModel(1, null);
    thread.Update(new ThreadSample(1, TimeSpan.Zero, ThreadState.Wait,
        ThreadWaitReason.UserRequest, 8, 8, null));

    Assert.Equal("Wait (UserRequest)", thread.StateLabel);
  }

  [Fact]
  public void Non_wait_state_label_is_just_the_state() {
    var thread = new ThreadRowViewModel(1, null);
    thread.Update(Thread(1, ThreadState.Running));

    Assert.Equal("Running", thread.StateLabel);
  }

  [Fact]
  public void End_task_terminates_the_given_row_regardless_of_selection() {
    StaRunner.Run(() => {
      var model = new FakeProcessModel();
      var controller = new FakeProcessController();
      var vm = new ProcessListViewModel(model, InertStats(), controller: controller);
      model.Samples.OnNext([Sample(100, "alpha"), Sample(200, "beta")]);
      vm.SelectedRow = vm.Rows.Single(r => r.ProcessId == 100);

      // The context menu targets the right-clicked row (200), not the selected one (100).
      vm.EndTask(vm.Rows.Single(r => r.ProcessId == 200));

      Assert.Equal(200u, controller.EndedPid);
    });
  }
}
