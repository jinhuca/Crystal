using System.Runtime.InteropServices;

namespace Crystal.Information.Cpu.Implementations;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CpuInstructionInfo {
  [MarshalAs(UnmanagedType.U1)]
  public bool _3DNOW;
  [MarshalAs(UnmanagedType.U1)]
  public bool _3DNOWEXT;
  [MarshalAs(UnmanagedType.U1)]
  public bool ABM;
  [MarshalAs(UnmanagedType.U1)]
  public bool ADX;
  [MarshalAs(UnmanagedType.U1)]
  public bool AES;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX2;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX512CD;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX512ER;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX512F;
  [MarshalAs(UnmanagedType.U1)]
  public bool AVX512PF;
  [MarshalAs(UnmanagedType.U1)]
  public bool BMI1;
  [MarshalAs(UnmanagedType.U1)]
  public bool BMI2;
  [MarshalAs(UnmanagedType.U1)]
  public bool CLFSH;
  [MarshalAs(UnmanagedType.U1)]
  public bool CMPXCHG16B;
  [MarshalAs(UnmanagedType.U1)]
  public bool CX8;
  [MarshalAs(UnmanagedType.U1)]
  public bool ERMS;
  [MarshalAs(UnmanagedType.U1)]
  public bool F16C;
  [MarshalAs(UnmanagedType.U1)]
  public bool FMA;
  [MarshalAs(UnmanagedType.U1)]
  public bool FSGSBASE;
  [MarshalAs(UnmanagedType.U1)]
  public bool FXSR;
  [MarshalAs(UnmanagedType.U1)]
  public bool HLE;
  [MarshalAs(UnmanagedType.U1)]
  public bool INVPCID;
  [MarshalAs(UnmanagedType.U1)]
  public bool LAHF;
  [MarshalAs(UnmanagedType.U1)]
  public bool LZCNT;
  [MarshalAs(UnmanagedType.U1)]
  public bool MMX;
  [MarshalAs(UnmanagedType.U1)]
  public bool MMXEXT;
  [MarshalAs(UnmanagedType.U1)]
  public bool MONITOR;
  [MarshalAs(UnmanagedType.U1)]
  public bool MOVBE;
  [MarshalAs(UnmanagedType.U1)]
  public bool MSR;
  [MarshalAs(UnmanagedType.U1)]
  public bool OSXSAVE;
  [MarshalAs(UnmanagedType.U1)]
  public bool PCLMULQDQ;
  [MarshalAs(UnmanagedType.U1)]
  public bool POPCNT;
  [MarshalAs(UnmanagedType.U1)]
  public bool PREFETCHWT1;
  [MarshalAs(UnmanagedType.U1)]
  public bool RDRAND;
  [MarshalAs(UnmanagedType.U1)]
  public bool RDSEED;
  [MarshalAs(UnmanagedType.U1)]
  public bool RDTSCP;
  [MarshalAs(UnmanagedType.U1)]
  public bool RTM;
  [MarshalAs(UnmanagedType.U1)]
  public bool SEP;
  [MarshalAs(UnmanagedType.U1)]
  public bool SHA;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE2;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE3;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE41;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE42;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSE4a;
  [MarshalAs(UnmanagedType.U1)]
  public bool SSSE3;
  [MarshalAs(UnmanagedType.U1)]
  public bool SYSCALL;
  [MarshalAs(UnmanagedType.U1)]
  public bool TBM;
  [MarshalAs(UnmanagedType.U1)]
  public bool XOP;
  [MarshalAs(UnmanagedType.U1)]
  public bool XSAVE;
}
