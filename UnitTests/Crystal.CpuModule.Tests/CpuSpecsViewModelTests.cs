using Crystal.CpuModule.ViewModels.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Xunit;

namespace Crystal.CpuModule.Tests;

public class CpuSpecsViewModelTests {
  [Fact]
  public void Update_copies_identity_fields_and_offsets_socket_to_one_based() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs {
      VendorName = "GenuineIntel",
      BrandName = "Core i7-9700K",
      FamilyId = 6,
      ModelId = 158,
      SteppingId = 12,
      BaseSpeed = 3600,
      BusSpeed = 100,
    }, socketIndex: 0));

    Assert.Equal("GenuineIntel", vm.Vendor);
    Assert.Equal("Core i7-9700K", vm.Brand);
    // SocketIndex 0 is displayed as socket 1.
    Assert.Equal(1, vm.Socket);
    Assert.Equal(6, vm.Family);
    Assert.Equal(158, vm.Model);
    Assert.Equal(12, vm.Stepping);
    Assert.Equal(3600, vm.BaseSpeedMHz);
    Assert.Equal(100, vm.BusSpeedMHz);
  }

  [Fact]
  public void Update_prefers_virtualization_enabled_over_supported() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs {
      VirtualizationSupported = true,
      VirtualizationEnabled = false,
    }));

    // Enabled is the authoritative live state; supported is only the fallback.
    Assert.False(vm.Virtualization);
  }

  [Fact]
  public void Update_falls_back_to_virtualization_supported_when_enabled_unknown() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs {
      VirtualizationSupported = true,
      VirtualizationEnabled = null,
    }));

    Assert.True(vm.Virtualization);
  }

  [Fact]
  public void Update_converts_cache_totals_from_bytes_to_kilobytes() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs {
      CacheInfo = Fakes.Cache(l1Bytes: 65536, l2Bytes: 524288, l3Bytes: 12582912, l1LineSize: 64),
    }));

    Assert.Equal(64, vm.L1CacheKb);      // 65536 / 1024
    Assert.Equal(512, vm.L2CacheKb);     // 524288 / 1024
    Assert.Equal(12288, vm.L3CacheKb);   // 12582912 / 1024
    Assert.Equal(64, vm.LineSizeBytes);
  }

  [Fact]
  public void Update_without_cache_info_leaves_cache_fields_at_default() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs { CacheInfo = null }));

    Assert.Equal(0, vm.L1CacheKb);
    Assert.Equal(0, vm.LineSizeBytes);
  }

  [Fact]
  public void Update_populates_instruction_set_in_fixed_order_with_availability() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs {
      InstructionSet = new CpuInstructionInfo { AVX = true, AVX2 = true, SSE = true, AVX512F = false },
    }));

    Assert.NotEmpty(vm.InstructionSet);
    var avx = Assert.Single(vm.InstructionSet, f => f.Name == "AVX");
    Assert.True(avx.IsAvailable);
    Assert.True(Assert.Single(vm.InstructionSet, f => f.Name == "SSE").IsAvailable);
    Assert.False(Assert.Single(vm.InstructionSet, f => f.Name == "AVX512F").IsAvailable);
  }

  [Fact]
  public void Update_repopulates_instruction_set_without_duplicating() {
    var vm = new CpuSpecsViewModel();
    var withIsa = new CpuSpecs { InstructionSet = new CpuInstructionInfo { AVX = true } };

    vm.Update(Fakes.System(specs: withIsa));
    int firstCount = vm.InstructionSet.Count;
    vm.Update(Fakes.System(specs: withIsa));

    // The list is cleared before repopulating — a second poll must not double the rows.
    Assert.Equal(firstCount, vm.InstructionSet.Count);
  }

  [Fact]
  public void Update_without_instruction_info_leaves_instruction_set_empty() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.System(specs: new CpuSpecs { InstructionSet = null }));

    Assert.Empty(vm.InstructionSet);
  }

  [Fact]
  public void Update_with_no_socket_is_a_noop() {
    var vm = new CpuSpecsViewModel();

    vm.Update(Fakes.Empty());

    Assert.Null(vm.Vendor);
    Assert.Null(vm.Socket);
    Assert.Empty(vm.InstructionSet);
  }
}
