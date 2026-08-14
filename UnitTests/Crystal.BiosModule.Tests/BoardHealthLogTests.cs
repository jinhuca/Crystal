using Crystal.BiosModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Crystal.BiosModule.Tests;

public class BoardHealthLogTests {
  // A hand-advanced clock so episode timestamps and durations are deterministic.
  private sealed class FakeClock {
    private DateTimeOffset _now = new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    public DateTimeOffset Now() => _now;
    public void Advance(TimeSpan by) => _now += by;
  }

  // Offenders now carry the formatted reading; most tests don't care about the value, so this helper
  // defaults it to empty. Tests exercising peak-value capture build the tuples inline instead.
  private static (string, ReadingSeverity, string)[] Offenders(params (string Name, ReadingSeverity Severity)[] rows) =>
      rows.Select(r => (r.Name, r.Severity, "")).ToArray();

  [Fact]
  public void An_out_of_spec_sensor_opens_a_single_ongoing_episode() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));

    var e = Assert.Single(log.Snapshot());
    Assert.Equal("+12V", e.SensorName);
    Assert.True(e.Ongoing);
    Assert.Null(e.EndedAt);
  }

  [Fact]
  public void An_episode_keeps_the_worst_severity_it_ever_reached() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+12V", ReadingSeverity.Critical)));  // escalates
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));   // eases but doesn't downgrade the peak

    Assert.Equal(ReadingSeverity.Critical, Assert.Single(log.Snapshot()).Severity);
  }

  [Fact]
  public void An_episode_pins_the_reading_captured_when_it_first_reached_its_worst_severity() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe([("+12V", ReadingSeverity.Warning, "11.3 V")]);
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe([("+12V", ReadingSeverity.Critical, "10.4 V")]);  // escalates → capture here
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe([("+12V", ReadingSeverity.Critical, "10.7 V")]);  // still critical, don't re-pin

    var e = Assert.Single(log.Snapshot());
    Assert.Equal(ReadingSeverity.Critical, e.Severity);
    Assert.Equal("10.4 V", e.PeakValue);
  }

  [Fact]
  public void Peak_timestamp_marks_when_the_worst_severity_was_first_reached_not_the_start() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    var start = clock.Now();
    log.Observe([("+12V", ReadingSeverity.Warning, "11.3 V")]);   // opens here
    clock.Advance(TimeSpan.FromSeconds(4));
    var escalatedAt = clock.Now();
    log.Observe([("+12V", ReadingSeverity.Critical, "10.4 V")]);  // peak reached here
    clock.Advance(TimeSpan.FromSeconds(2));
    log.Observe([("+12V", ReadingSeverity.Critical, "10.7 V")]);  // still critical, don't re-stamp

    var e = Assert.Single(log.Snapshot());
    Assert.Equal(escalatedAt, e.PeakAt);
    Assert.NotEqual(start, e.PeakAt);
  }

  [Fact]
  public void A_first_seen_offender_captures_its_opening_reading_as_the_peak() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe([("VBAT", ReadingSeverity.Warning, "2.6 V")]);

    Assert.Equal("2.6 V", Assert.Single(log.Snapshot()).PeakValue);
  }

  [Fact]
  public void Recovery_closes_the_episode_and_dates_the_end_to_the_last_bad_tick() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    var start = clock.Now();
    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(3));
    var lastBad = clock.Now();
    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(5));            // idle gap after recovery
    log.Observe(Offenders());                          // nothing out of spec → closes

    var e = Assert.Single(log.Snapshot());
    Assert.False(e.Ongoing);
    Assert.Equal(start, e.StartedAt);
    Assert.Equal(lastBad, e.EndedAt);                  // not "now" — the idle gap isn't counted
  }

  [Fact]
  public void A_sensor_that_recovers_then_reoffends_opens_a_second_episode() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders());                          // recovers → episode 1 closes
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+12V", ReadingSeverity.Critical)));  // reoffends → episode 2 opens

    var events = log.Snapshot();
    Assert.Equal(2, events.Count);
    Assert.True(events[0].Ongoing);                    // ongoing listed first
    Assert.Equal(ReadingSeverity.Critical, events[0].Severity);
    Assert.False(events[1].Ongoing);
    Assert.Equal(ReadingSeverity.Warning, events[1].Severity);
  }

  [Fact]
  public void Multiple_ongoing_episodes_are_ordered_newest_start_first() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders(("+5V", ReadingSeverity.Warning)));
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+5V", ReadingSeverity.Warning), ("+12V", ReadingSeverity.Critical)));

    var events = log.Snapshot();
    Assert.Equal("+12V", events[0].SensorName);        // started later → on top
    Assert.Equal("+5V", events[1].SensorName);
  }

  [Fact]
  public void Closed_episodes_are_capped_and_drop_oldest_first() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    // Open+close 60 distinct episodes; only the newest 50 recovered ones should survive.
    for (int i = 0; i < 60; i++) {
      log.Observe(Offenders(($"rail{i}", ReadingSeverity.Warning)));
      clock.Advance(TimeSpan.FromSeconds(1));
      log.Observe(Offenders());
      clock.Advance(TimeSpan.FromSeconds(1));
    }

    var events = log.Snapshot();
    Assert.Equal(50, events.Count);
    Assert.DoesNotContain(events, e => e.SensorName == "rail0");
    Assert.Contains(events, e => e.SensorName == "rail59");
    Assert.Equal(10, log.DroppedCount);                // 60 closed − 50 kept
  }

  [Fact]
  public void Dropped_count_is_zero_until_the_cap_is_exceeded_and_resets_on_clear() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    for (int i = 0; i < 55; i++) {
      log.Observe(Offenders(($"rail{i}", ReadingSeverity.Warning)));
      clock.Advance(TimeSpan.FromSeconds(1));
      log.Observe(Offenders());
      clock.Advance(TimeSpan.FromSeconds(1));
    }
    Assert.Equal(5, log.DroppedCount);

    log.Clear();
    Assert.Equal(0, log.DroppedCount);
  }

  [Fact]
  public void Clear_drops_ongoing_and_recovered_episodes_alike() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders(("+12V", ReadingSeverity.Warning)));  // ongoing
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders());                                    // recovered → closed
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+5V", ReadingSeverity.Critical)));   // another ongoing
    Assert.NotEmpty(log.Snapshot());

    log.Clear();

    Assert.Empty(log.Snapshot());

    // A still-bad sensor simply opens a fresh episode on the next tick.
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders(("+5V", ReadingSeverity.Critical)));
    Assert.Single(log.Snapshot());
  }

  [Fact]
  public void No_offenders_ever_means_no_events() {
    var clock = new FakeClock();
    var log = new BoardHealthLog(clock.Now);

    log.Observe(Offenders());
    clock.Advance(TimeSpan.FromSeconds(1));
    log.Observe(Offenders());

    Assert.Empty(log.Snapshot());
  }
}
