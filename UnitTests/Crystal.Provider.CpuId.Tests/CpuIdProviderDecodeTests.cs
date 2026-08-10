using Crystal.Provider.CpuId;
using Xunit;

namespace Crystal.Provider.CpuId.Tests;

/// <summary>
/// Covers the pure CPUID register decoders in <see cref="CpuIdProvider"/> (family/model/stepping
/// packing, the little-endian vendor/brand string layout, and the feature-flag bit map). The
/// <see cref="CpuIdProvider.Query"/> path itself executes the CPUID instruction and is hardware.
/// </summary>
public class CpuIdProviderDecodeTests {
  [Fact]
  public void DecodeFms_IntelSkylake_UsesExtendedModel() {
    // EAX 0x000506E3: family 6, model 0x5E (extModel 5 << 4 | baseModel 0xE), stepping 3.
    var (family, model, stepping) = CpuIdProvider.DecodeFms(0x000506E3);

    Assert.Equal(6u, family);
    Assert.Equal(0x5Eu, model);
    Assert.Equal(3u, stepping);
  }

  [Fact]
  public void DecodeFms_AmdZen2_AddsExtendedFamily() {
    // EAX 0x00870F10: baseFamily 0xF + extFamily 0x8 = 0x17 (23); model 0x71; stepping 0.
    var (family, model, stepping) = CpuIdProvider.DecodeFms(0x00870F10);

    Assert.Equal(0x17u, family);
    Assert.Equal(0x71u, model);
    Assert.Equal(0u, stepping);
  }

  [Fact]
  public void DecodeFms_LegacyFamily_IgnoresExtendedModel() {
    // baseFamily 5 is neither 6 nor 0xF, so extModel is not folded in: model == baseModel.
    // EAX 0x00050521: extModel 5, baseFamily 5, baseModel 2, stepping 1.
    var (family, model, stepping) = CpuIdProvider.DecodeFms(0x00050521);

    Assert.Equal(5u, family);
    Assert.Equal(2u, model);   // extModel 5 ignored
    Assert.Equal(1u, stepping);
  }

  [Fact]
  public void DecodeVendor_IntelRegisters_ProduceGenuineIntel() {
    // EBX="Genu", EDX="ineI", ECX="ntel" (little-endian ASCII).
    Assert.Equal("GenuineIntel",
        CpuIdProvider.DecodeVendor(ebx: 0x756E6547, edx: 0x49656E69, ecx: 0x6C65746E));
  }

  [Fact]
  public void DecodeVendor_AmdRegisters_ProduceAuthenticAMD() {
    // EBX="Auth", EDX="enti", ECX="cAMD".
    Assert.Equal("AuthenticAMD",
        CpuIdProvider.DecodeVendor(ebx: 0x68747541, edx: 0x69746E65, ecx: 0x444D4163));
  }

  [Fact]
  public void DecodeInstructionSet_MapsLeaf1Edx_And_Leaf1Ecx_Bits() {
    // Leaf1 EDX: SSE (25), SSE2 (26). Leaf1 ECX: AVX (28), SSE3 (0).
    uint f1Edx = (1u << 25) | (1u << 26);
    uint f1Ecx = (1u << 28) | (1u << 0);

    var info = CpuIdProvider.DecodeInstructionSet(f1Ecx, f1Edx, f7Ebx: 0, f7Ecx: 0, eEcx: 0, eEdx: 0);

    Assert.True(info.SSE);
    Assert.True(info.SSE2);
    Assert.True(info.AVX);
    Assert.True(info.SSE3);
    Assert.False(info.AVX2);   // leaf 7 not set
    Assert.False(info.MMX);    // EDX bit 23 not set
  }

  [Fact]
  public void DecodeInstructionSet_MapsLeaf7_And_ExtendedLeafBits() {
    uint f7Ebx = (1u << 5) | (1u << 29);   // AVX2, SHA
    uint eEcx = (1u << 5);                  // ABM and LZCNT share bit 5
    uint eEdx = (1u << 31) | (1u << 11);    // _3DNOW, SYSCALL

    var info = CpuIdProvider.DecodeInstructionSet(f1Ecx: 0, f1Edx: 0, f7Ebx, f7Ecx: 0, eEcx, eEdx);

    Assert.True(info.AVX2);
    Assert.True(info.SHA);
    Assert.True(info.ABM);
    Assert.True(info.LZCNT);
    Assert.True(info._3DNOW);
    Assert.True(info.SYSCALL);
    Assert.False(info.SSE);   // no leaf-1 bits set
  }

  [Fact]
  public void DecodeInstructionSet_AllZero_LeavesEveryFlagFalse() {
    var info = CpuIdProvider.DecodeInstructionSet(0, 0, 0, 0, 0, 0);

    Assert.False(info.SSE);
    Assert.False(info.AVX);
    Assert.False(info.AVX512F);
    Assert.False(info.RDRAND);
    Assert.False(info.SYSCALL);
  }
}
