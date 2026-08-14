using Crystal.BiosModule.ViewModels;
using System;
using Xunit;

namespace Crystal.BiosModule.Tests;

public class BoardHealthEventViewModelTests {
  private static readonly DateTimeOffset Start = new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public void Sort_keys_are_numeric_so_ordering_is_chronological_and_by_length() {
    var peakAt = Start + TimeSpan.FromSeconds(30);
    var e = new BoardHealthEvent("+12V", ReadingSeverity.Critical, "10.4 V",
        Start, peakAt, Start + TimeSpan.FromSeconds(95));
    var now = Start + TimeSpan.FromMinutes(5);

    var vm = new BoardHealthEventViewModel(e, now);

    Assert.Equal(Start.UtcTicks, vm.StartedSort);
    Assert.Equal(peakAt.UtcTicks, vm.PeakAtSort);   // peak-at sorts on its own tick key, not the start
    Assert.Equal(95d, vm.DurationSort, 3);   // recovered → span is fixed at its recorded length
    Assert.Equal("1m 35s", vm.Duration);     // and the numeric key matches the formatted string
  }

  [Fact]
  public void Ongoing_duration_key_grows_with_now_so_it_sorts_by_current_age() {
    var e = new BoardHealthEvent("+5V", ReadingSeverity.Warning, "5.4 V", Start, Start, EndedAt: null);
    var now = Start + TimeSpan.FromSeconds(42);

    var vm = new BoardHealthEventViewModel(e, now);

    Assert.True(vm.Ongoing);
    Assert.Equal(42d, vm.DurationSort, 3);   // measured against 'now', not a fixed end
  }

  [Theory]
  [InlineData(0, "<1s ago")]
  [InlineData(1, "1s ago")]
  [InlineData(42, "42s ago")]
  [InlineData(150, "2m ago")]
  [InlineData(7200, "2h 0m ago")]
  public void Age_is_relative_to_now(int elapsedSeconds, string expected) {
    var e = new BoardHealthEvent("+12V", ReadingSeverity.Critical, "10.4 V", Start, Start, EndedAt: null);
    var now = Start + TimeSpan.FromSeconds(elapsedSeconds);

    var vm = new BoardHealthEventViewModel(e, now);

    Assert.Equal(expected, vm.Age);
  }

  [Fact]
  public void Age_ticks_forward_as_now_advances() {
    var e = new BoardHealthEvent("+5V", ReadingSeverity.Warning, "5.4 V", Start, Start, EndedAt: null);

    var early = new BoardHealthEventViewModel(e, Start + TimeSpan.FromSeconds(10));
    var later = new BoardHealthEventViewModel(e, Start + TimeSpan.FromMinutes(3));

    Assert.Equal("10s ago", early.Age);
    Assert.Equal("3m ago", later.Age);
  }

  [Fact]
  public void Age_clamps_to_just_now_when_clock_runs_backwards() {
    var e = new BoardHealthEvent("+3.3V", ReadingSeverity.Warning, "3.0 V", Start, Start, EndedAt: null);
    var now = Start - TimeSpan.FromSeconds(5);

    var vm = new BoardHealthEventViewModel(e, now);

    Assert.Equal("just now", vm.Age);
  }

  [Fact]
  public void Detail_lists_full_start_end_peak_and_duration_for_a_recovered_episode() {
    var peakAt = Start + TimeSpan.FromSeconds(10);   // peak reached 10s into the episode, not at start
    var e = new BoardHealthEvent("+12V", ReadingSeverity.Critical, "10.4 V",
        Start, peakAt, Start + TimeSpan.FromSeconds(95));
    var now = Start + TimeSpan.FromMinutes(5);

    var vm = new BoardHealthEventViewModel(e, now);
    var detail = vm.Detail;

    Assert.Contains("+12V — Critical", detail);
    Assert.Contains($"Peak reading: 10.4 V at {peakAt.LocalDateTime:HH:mm:ss}", detail);
    Assert.Equal(peakAt.LocalDateTime.ToString("HH:mm:ss"), vm.PeakAt);
    Assert.Contains($"Started: {Start.LocalDateTime:yyyy-MM-dd HH:mm:ss}", detail);
    Assert.Contains($"Ended: {(Start + TimeSpan.FromSeconds(95)).LocalDateTime:yyyy-MM-dd HH:mm:ss}", detail);
    Assert.Contains("Duration: 1m 35s", detail);
  }

  [Fact]
  public void Detail_reports_an_ongoing_episode_as_not_yet_ended() {
    var e = new BoardHealthEvent("+5V", ReadingSeverity.Warning, "5.4 V", Start, Start, EndedAt: null);
    var now = Start + TimeSpan.FromSeconds(42);

    var detail = new BoardHealthEventViewModel(e, now).Detail;

    Assert.Contains("Ended: ongoing", detail);
    Assert.Contains("Duration: ongoing · 42s", detail);
  }
}
