using System.Reactive.Subjects;
using Crystal.Service.Memory;
using MemoryModule.Models;
using MemoryModule.ViewModels;
using Prism.Events;
using Xunit;

namespace MemoryModule.Tests;

public class MemoryViewModelTests {
  private sealed class FakeMemoryModel : IMemoryModel {
    public Subject<MemorySnapshot> SpecsSubject { get; } = new();
    public Subject<MemoryLoadReading> LoadSubject { get; } = new();
    public IObservable<MemorySnapshot> Specs => SpecsSubject;
    public IObservable<MemoryLoadReading> Load => LoadSubject;
  }

  private static MemoryViewModel CreateVm(out FakeMemoryModel model) {
    model = new FakeMemoryModel();
    return new MemoryViewModel(model, new EventAggregator());
  }

  private static MemoryModuleInfo Stick(string slot = "DIMM A1", double? gb = 16) =>
      new(SlotLabel: slot, CapacityGB: gb, SpeedMHz: 6000, ConfiguredSpeedMHz: 5600,
          FormFactor: "DIMM", Manufacturer: "Corsair", PartNumber: "CMK32", SerialNumber: "SN1");

  private static MemorySnapshot Specs(double? totalGB = 32, int populated = 2, uint? speed = 6000,
                                      string? type = "DDR5", string? formFactor = "DIMM",
                                      int? totalSlots = 4) =>
      new(Modules: [Stick("DIMM A1"), Stick("DIMM B1")], TotalCapacityGB: totalGB,
          PopulatedSlots: populated, MaxSpeedMHz: speed, MemoryType: type,
          FormFactor: formFactor, TotalSlots: totalSlots);

  [Fact]
  public void Header_combines_total_and_memory_type() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(totalGB: 32, type: "DDR5"));

    Assert.Equal("32 GB DDR5", vm.HeaderSpecLabel);
  }

  [Fact]
  public void Header_omits_type_when_unknown() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(totalGB: 32, type: null));

    Assert.Equal("32 GB", vm.HeaderSpecLabel);
  }

  [Fact]
  public void Header_is_placeholder_when_capacity_unknown() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(totalGB: null));

    Assert.Equal("—", vm.HeaderSpecLabel);
  }

  [Fact]
  public void Spec_labels_format_speed_slots_and_form_factor() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(speed: 6000, populated: 2, totalSlots: 4, formFactor: "DIMM"));

    Assert.Equal("6000 MT/s", vm.SpeedLabel);
    Assert.Equal("2 of 4", vm.SlotsUsedLabel);
    Assert.Equal("DIMM", vm.FormFactorLabel);
  }

  [Fact]
  public void Slots_used_omits_total_when_slot_count_unknown() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(populated: 2, totalSlots: null));

    Assert.Equal("2", vm.SlotsUsedLabel);
  }

  [Fact]
  public void Summary_header_labels_report_total_slots_and_speed() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(totalGB: 64, populated: 4, speed: 2133));

    // These three feed the summary tile's inline header (total · populated · max speed).
    Assert.Equal("64 GB", vm.TotalCapacityLabel);
    Assert.Equal("4 populated", vm.SlotsLabel);
    Assert.Equal("2133 MHz", vm.MaxSpeedLabel);
  }

  [Fact]
  public void Summary_max_speed_is_placeholder_when_unknown() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs(speed: null));

    Assert.Equal("—", vm.MaxSpeedLabel);
  }

  [Fact]
  public void Specs_populate_the_module_list() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Specs());

    Assert.Equal(2, vm.Modules.Count);
    Assert.Equal("DIMM A1", vm.Modules[0].SlotLabel);
  }

  [Fact]
  public void Load_sets_in_use_and_available_labels() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs(totalGB: 32));

    model.LoadSubject.OnNext(new MemoryLoadReading(
        LoadPercent: 50, UsedGB: 16, AvailableGB: 16));

    Assert.Equal("16 GB", vm.InUseLabel);
    Assert.Equal("16 GB", vm.AvailableLabel);
    Assert.Equal(50, vm.Load);
  }

  [Fact]
  public void Committed_shows_used_over_limit() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs());

    model.LoadSubject.OnNext(new MemoryLoadReading(
        LoadPercent: 50, UsedGB: 16, AvailableGB: 16,
        CommittedGB: 20.4, CommitLimitGB: 40.9));

    Assert.Equal("20.4/40.9 GB", vm.CommittedLabel);
  }

  [Fact]
  public void Composition_fraction_is_used_over_capacity_clamped() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs(totalGB: 32));

    model.LoadSubject.OnNext(new MemoryLoadReading(LoadPercent: 25, UsedGB: 8, AvailableGB: 24));

    Assert.Equal(0.25, vm.CompositionInUseFraction, precision: 3);
  }

  [Fact]
  public void Composition_fraction_clamps_to_one_when_used_exceeds_capacity() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs(totalGB: 32));

    // Used above the reported capacity (rounding/hardware-reserved skew) must not overflow the bar.
    model.LoadSubject.OnNext(new MemoryLoadReading(LoadPercent: 100, UsedGB: 40, AvailableGB: 0));

    Assert.Equal(1.0, vm.CompositionInUseFraction, precision: 3);
  }

  [Fact]
  public void Hardware_reserved_shows_gb_above_one_gb() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs());

    model.LoadSubject.OnNext(new MemoryLoadReading(
        LoadPercent: 50, UsedGB: 16, AvailableGB: 16, HardwareReservedGB: 1.5));

    Assert.Equal("1.5 GB", vm.HardwareReservedLabel);
  }

  [Fact]
  public void Hardware_reserved_shows_mb_below_one_gb() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs());

    // 0.25 GB -> 256 MB.
    model.LoadSubject.OnNext(new MemoryLoadReading(
        LoadPercent: 50, UsedGB: 16, AvailableGB: 16, HardwareReservedGB: 0.25));

    Assert.Equal("256 MB", vm.HardwareReservedLabel);
  }

  [Fact]
  public void Missing_kernel_stats_show_placeholder() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Specs());

    model.LoadSubject.OnNext(new MemoryLoadReading(LoadPercent: 50, UsedGB: 16, AvailableGB: 16));

    Assert.Equal("—", vm.CachedLabel);
    Assert.Equal("—", vm.PagedPoolLabel);
    Assert.Equal("—", vm.NonPagedPoolLabel);
    Assert.Equal("—", vm.HardwareReservedLabel);
  }
}
