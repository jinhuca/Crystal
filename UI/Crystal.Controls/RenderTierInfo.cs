using System.Windows.Media;

namespace Crystal.Controls;

/// <summary>
/// Thin wrapper over <see cref="RenderCapability.Tier"/>. Tier ≥ 2 means shader effects
/// (BlurEffect/DropShadowEffect) run on the GPU; tier 0/1 means they fall back to the CPU, where a
/// large Gaussian blur is expensive.
/// </summary>
public static class RenderTierInfo {
  /// <summary>
  /// RenderCapability.Tier packs the tier in the high word.
  /// </summary>
  public static int Tier => RenderCapability.Tier >> 16;

  /// <summary>
  /// True if the current system can run shader effects on the GPU, false if they fall back to the CPU.
  /// </summary>
  public static bool IsHardwareAccelerated => Tier >= 2;

  /// <summary>
  /// Returns a human-readable description of the current render tier.
  /// </summary>
  /// <returns></returns>
  public static string Describe() => IsHardwareAccelerated
    ? $"GPU accelerated (tier {Tier})"
    : $"SOFTWARE (tier {Tier}) — blur disabled";
}
