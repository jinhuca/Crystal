using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Crystal.Controls;

/// <summary>
/// Thin wrapper over <see cref="RenderCapability.Tier"/>. Tier ≥ 2 means shader effects
/// (BlurEffect/DropShadowEffect) run on the GPU; tier 0/1 means they fall back to the CPU, where a
/// large Gaussian blur is expensive.
/// </summary>
public static class RenderTierInfo {
  // RenderCapability.Tier packs the tier in the high word.
  public static int Tier => RenderCapability.Tier >> 16;

  public static bool IsHardwareAccelerated => Tier >= 2;

  public static string Describe() => IsHardwareAccelerated
      ? $"GPU accelerated (tier {Tier})"
      : $"SOFTWARE (tier {Tier}) — blur disabled";
}
