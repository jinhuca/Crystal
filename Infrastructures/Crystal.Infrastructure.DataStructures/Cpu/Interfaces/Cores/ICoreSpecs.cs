using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

public interface ICoreSpecs {
  int CoreIndex { get; init; }            // matches LHM's/topology-leaf's core numbering
  int? ApicId { get; init; }              // initial x2APIC ID (CPUID topology leaf) - useful for
                                          // correlating this core across CPUID/LHM/OS core numbering
  CoreType? Type { get; init; }           // Performance / Efficiency / Unknown - CPUID leaf 0x1A
                                          // (hybrid info, Alder Lake+) - directly closes the hybrid-
                                          // topology gap flagged as a limitation in CpuIdentity.cpp
  int? ThreadCount { get; init; }         // 1 or 2 typically - threads sharing this physical core
  float? MaxTurboFrequency { get; init; } // per-core turbo bin - varies core-to-core on CPUs with
                                          // favored-core boosting (Intel TVB / AMD PPT-based boost)
}
