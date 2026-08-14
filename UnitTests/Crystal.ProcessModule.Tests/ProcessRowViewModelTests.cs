using Crystal.ProcessModule.ViewModels;
using Crystal.Service.Process;
using Xunit;

namespace Crystal.ProcessModule.Tests;

public class ProcessRowViewModelTests {
  private static ProcessSample Sample(double cpu, double mem) =>
      new(100, "alpha", cpu, mem, ProcessCategory.BackgroundProcess);

  [Fact]
  public void First_update_seeds_the_peaks_from_the_current_reading() {
    var row = new ProcessRowViewModel(100, "alpha");

    row.Update(Sample(cpu: 12.5, mem: 200));

    Assert.Equal(12.5, row.PeakCpuPercent);
    Assert.Equal(200, row.PeakWorkingSetMb);
  }

  [Fact]
  public void Peaks_rise_to_a_new_high() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 12.5, mem: 200));

    row.Update(Sample(cpu: 40, mem: 512));

    Assert.Equal(40, row.PeakCpuPercent);
    Assert.Equal(512, row.PeakWorkingSetMb);
  }

  [Fact]
  public void Peaks_hold_when_the_current_reading_dips_back_down() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 40, mem: 512));

    row.Update(Sample(cpu: 5, mem: 128));

    Assert.Equal(5, row.CpuPercent);          // live value follows the dip
    Assert.Equal(128, row.WorkingSetMb);
    Assert.Equal(40, row.PeakCpuPercent);     // peak stays at the high-water mark
    Assert.Equal(512, row.PeakWorkingSetMb);
  }

  [Fact]
  public void Cpu_and_memory_peaks_track_independently() {
    var row = new ProcessRowViewModel(100, "alpha");

    row.Update(Sample(cpu: 90, mem: 100));   // CPU spikes, memory low
    row.Update(Sample(cpu: 10, mem: 900));   // CPU low, memory spikes

    Assert.Equal(90, row.PeakCpuPercent);
    Assert.Equal(900, row.PeakWorkingSetMb);
  }

  [Theory]
  [InlineData(49.9, false)]
  [InlineData(50, true)]    // at the threshold → flagged
  [InlineData(95, true)]
  public void Cpu_hog_flag_trips_at_the_threshold(double peakCpu, bool expected) {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: peakCpu, mem: 0));

    Assert.Equal(expected, row.IsSustainedCpuHog);
  }

  [Theory]
  [InlineData(1023, false)]
  [InlineData(1024, true)]   // at the threshold → flagged
  [InlineData(4096, true)]
  public void Memory_hog_flag_trips_at_the_threshold(double peakMem, bool expected) {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 0, mem: peakMem));

    Assert.Equal(expected, row.IsMemoryHog);
  }

  [Fact]
  public void Hog_flags_stay_set_after_the_live_reading_dips_below_the_threshold() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 90, mem: 2048));   // trips both
    row.Update(Sample(cpu: 1, mem: 50));      // live dips, peaks hold

    Assert.True(row.IsSustainedCpuHog);
    Assert.True(row.IsMemoryHog);
  }

  [Fact]
  public void Resetting_peaks_clears_the_hog_flags_when_current_is_low() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 90, mem: 2048));   // trips both
    row.Update(Sample(cpu: 1, mem: 50));      // live low, peaks still high

    row.ResetPeaks();

    Assert.False(row.IsSustainedCpuHog);
    Assert.False(row.IsMemoryHog);
  }

  [Fact]
  public void Crossing_a_threshold_notifies_the_hog_flag() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 10, mem: 100));

    var changed = new List<string?>();
    row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

    row.Update(Sample(cpu: 70, mem: 2048));

    Assert.Contains(nameof(ProcessRowViewModel.IsSustainedCpuHog), changed);
    Assert.Contains(nameof(ProcessRowViewModel.IsMemoryHog), changed);
  }

  [Fact]
  public void Raising_a_peak_notifies_the_bound_property() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 10, mem: 100));

    var changed = new List<string?>();
    row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

    row.Update(Sample(cpu: 50, mem: 400));

    Assert.Contains(nameof(ProcessRowViewModel.PeakCpuPercent), changed);
    Assert.Contains(nameof(ProcessRowViewModel.PeakWorkingSetMb), changed);
  }

  [Fact]
  public void Reset_peaks_collapses_both_peaks_to_the_current_reading() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 90, mem: 900));   // peaks high
    row.Update(Sample(cpu: 5, mem: 128));    // live dipped, peaks still 90/900

    row.ResetPeaks();

    Assert.Equal(5, row.PeakCpuPercent);     // collapsed to current live value
    Assert.Equal(128, row.PeakWorkingSetMb);
  }

  [Fact]
  public void Peaks_reestablish_from_live_values_after_a_reset() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 90, mem: 900));
    row.Update(Sample(cpu: 5, mem: 128));
    row.ResetPeaks();

    row.Update(Sample(cpu: 30, mem: 300));   // new high relative to the post-reset baseline

    Assert.Equal(30, row.PeakCpuPercent);
    Assert.Equal(300, row.PeakWorkingSetMb);
  }

  [Fact]
  public void Executable_path_label_shows_the_path_when_known() {
    var row = new ProcessRowViewModel(100, "alpha");

    row.Update(new ProcessSample(100, "alpha", 5, 100, ProcessCategory.BackgroundProcess,
        ExecutablePath: @"C:\Windows\System32\alpha.exe"));

    Assert.Equal(@"C:\Windows\System32\alpha.exe", row.ExecutablePathLabel);
  }

  [Fact]
  public void Executable_path_label_is_placeholder_when_path_unknown() {
    var row = new ProcessRowViewModel(100, "alpha");

    // WMI couldn't read the image path (protected/system process): sample carries no path.
    row.Update(Sample(cpu: 5, mem: 100));

    Assert.Equal("Path unavailable", row.ExecutablePathLabel);
  }

  [Fact]
  public void Setting_the_executable_path_notifies_its_label() {
    var row = new ProcessRowViewModel(100, "alpha");

    var changed = new List<string?>();
    row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

    row.ExecutablePath = @"C:\app\alpha.exe";

    Assert.Contains(nameof(ProcessRowViewModel.ExecutablePathLabel), changed);
  }

  [Fact]
  public void A_dip_does_not_notify_the_peak_properties() {
    var row = new ProcessRowViewModel(100, "alpha");
    row.Update(Sample(cpu: 50, mem: 400));

    var changed = new List<string?>();
    row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

    row.Update(Sample(cpu: 5, mem: 50));

    Assert.DoesNotContain(nameof(ProcessRowViewModel.PeakCpuPercent), changed);
    Assert.DoesNotContain(nameof(ProcessRowViewModel.PeakWorkingSetMb), changed);
  }
}
