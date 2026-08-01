using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Crystal.DataStructures.Cpu.Definitions;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CpuCacheInfo {
  public int L1_cache_size;
  public int L1_cache_line_size;
  public int L2_cache_size;
  public int L2_cache_line_size;
  public int L3_cache_size;
  public int L3_cache_line_size;
}