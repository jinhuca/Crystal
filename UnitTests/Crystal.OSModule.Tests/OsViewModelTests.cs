using Crystal.OSModule.Models;
using Crystal.OSModule.ViewModels;
using Prism.Events;
using System.Reactive.Subjects;
using Xunit;

namespace Crystal.OSModule.Tests;

public class OsViewModelTests {
  private sealed class FakeOsModel : IOsModel {
    public Subject<OsSnapshot> InfoSubject { get; } = new();
    public Subject<OsLiveReading> LiveSubject { get; } = new();
    public IObservable<OsSnapshot> Info => InfoSubject;
    public IObservable<OsLiveReading> Live => LiveSubject;
  }

  private static OsViewModel CreateVm(out FakeOsModel model) {
    model = new FakeOsModel();
    return new OsViewModel(model, new EventAggregator());
  }

  private static OsSnapshot Info(
      string? caption = "Windows 11 Pro",
      string? edition = "Professional",
      string? version = "10.0.22631",
      string? build = "22631.4169",
      string? displayVersion = "23H2",
      string? architecture = "X64",
      string? machineName = "DESK-1",
      string? userName = "alice") =>
      new(Caption: caption, Edition: edition, Version: version, BuildNumber: build,
          DisplayVersion: displayVersion, Architecture: architecture, MachineName: machineName,
          UserName: userName);

  [Fact]
  public void Info_maps_essential_summary_fields() {
    var vm = CreateVm(out var model);

    model.InfoSubject.OnNext(Info());

    Assert.Equal("Windows 11 Pro", vm.OsName);
    Assert.Equal("23H2", vm.DisplayVersion);
    Assert.Equal("22631.4169", vm.BuildLabel);
    Assert.Equal("X64", vm.Architecture);
  }

  [Fact]
  public void Info_maps_full_detail_identity_fields() {
    var vm = CreateVm(out var model);

    model.InfoSubject.OnNext(Info(edition: "Professional", version: "10.0.22631",
        machineName: "DESK-1", userName: "alice"));

    Assert.Equal("Professional", vm.Edition);
    Assert.Equal("10.0.22631", vm.VersionLabel);
    Assert.Equal("DESK-1", vm.MachineName);
    Assert.Equal("alice", vm.UserName);
  }

  [Fact]
  public void Missing_string_fields_show_placeholder() {
    var vm = CreateVm(out var model);

    model.InfoSubject.OnNext(Info(caption: null, build: null, displayVersion: null,
        machineName: null, userName: null));

    Assert.Equal("—", vm.OsName);
    Assert.Equal("—", vm.BuildLabel);
    Assert.Equal("—", vm.DisplayVersion);
    Assert.Equal("—", vm.MachineName);
    Assert.Equal("—", vm.UserName);
  }

  [Fact]
  public void Blank_string_fields_show_placeholder() {
    var vm = CreateVm(out var model);

    // Whitespace-only values from the registry read as "unknown", not a real blank.
    model.InfoSubject.OnNext(Info(caption: "   ", edition: ""));

    Assert.Equal("—", vm.OsName);
    Assert.Equal("—", vm.Edition);
  }

  [Fact]
  public void Install_and_boot_dates_format_when_present() {
    var vm = CreateVm(out var model);
    var install = new DateTimeOffset(2024, 3, 15, 9, 41, 0, TimeSpan.Zero);
    var boot = new DateTimeOffset(2024, 8, 1, 7, 5, 0, TimeSpan.Zero);

    model.InfoSubject.OnNext(new OsSnapshot(InstallDate: install, LastBootTime: boot));

    Assert.Equal("2024-03-15 09:41", vm.InstallDateLabel);
    Assert.Equal("2024-08-01 07:05", vm.LastBootTimeLabel);
  }

  [Fact]
  public void Missing_dates_show_placeholder() {
    var vm = CreateVm(out var model);

    model.InfoSubject.OnNext(new OsSnapshot(InstallDate: null, LastBootTime: null));

    Assert.Equal("—", vm.InstallDateLabel);
    Assert.Equal("—", vm.LastBootTimeLabel);
  }

  [Fact]
  public void Live_reading_sets_uptime_and_current_time() {
    var vm = CreateVm(out var model);
    var now = new DateTimeOffset(2024, 8, 4, 13, 22, 12, TimeSpan.Zero);

    model.LiveSubject.OnNext(new OsLiveReading(
        Uptime: new TimeSpan(days: 3, hours: 21, minutes: 22, seconds: 12), Now: now));

    Assert.Equal("3d 21:22:12", vm.UptimeLabel);
    Assert.Equal("2024-08-04 13:22:12", vm.CurrentTimeLabel);
  }
}
