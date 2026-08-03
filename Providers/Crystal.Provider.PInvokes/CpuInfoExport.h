#pragma once
// Link with PowrProf.lib
#pragma comment(lib, "PowrProf.lib")

#include "CpuInstructionSet.h"
#include <Windows.h>

#include <powersetting.h>
#include <powrprof.h>
#include <iostream>
#include <vector>
#include <intrin.h>
#include <wtypes.h>
#include <oleauto.h>

extern "C" __declspec(dllexport) BSTR __stdcall Brand() {
  std::string result = InstructionSet::Brand();
  std::wstring wide_text(result.begin(), result.end());
  return SysAllocString(wide_text.c_str());
}

extern "C" __declspec(dllexport) BSTR __stdcall Vendor() {
  std::string result = InstructionSet::Vendor();
  std::wstring wide_text(result.begin(), result.end());
  return SysAllocString(wide_text.c_str());
}

extern "C" __declspec(dllexport) bool VirtualizationEnabled() {
  int cpuInfo[4];
  // Check for Intel VT-x
  __cpuid(cpuInfo, 1);
  if(cpuInfo[2] & (1 << 5)) return true;

  // Check for AMD-V
  __cpuid(cpuInfo, 0x80000001);
  if(cpuInfo[2] & (1 << 2)) return true;

  return false;
}

struct InstructionSetStruct {
  bool _3DNOW;
  bool _3DNOWEXT;
  bool ABM;
  bool ADX;
  bool AES;
  bool AVX;
  bool AVX2;
  bool AVX512CD;
  bool AVX512ER;
  bool AVX512F;
  bool AVX512PF;
  bool BMI1;
  bool BMI2;
  bool CLFSH;
  bool CMPXCHG16B;
  bool CX8;
  bool ERMS;
  bool F16C;
  bool FMA;
  bool FSGSBASE;
  bool FXSR;
  bool HLE;
  bool INVPCID;
  bool LAHF;
  bool LZCNT;
  bool MMX;
  bool MMXEXT;
  bool MONITOR;
  bool MOVBE;
  bool MSR;
  bool OSXSAVE;
  bool PCLMULQDQ;
  bool POPCNT;
  bool PREFETCHWT1;
  bool RDRAND;
  bool RDSEED;
  bool RDTSCP;
  bool RTM;
  bool SEP;
  bool SHA;
  bool SSE;
  bool SSE2;
  bool SSE3;
  bool SSE41;
  bool SSE42;
  bool SSE4a;
  bool SSSE3;
  bool SYSCALL;
  bool TBM;
  bool XOP;
  bool XSAVE;
};

struct PROCESSOR_POWER_INFORMATION {
  ULONG Number;
  ULONG MaxMhz;
  ULONG CurrentMhz;
  ULONG MhzLimit;
  ULONG MaxIdleState;
  ULONG CurrentIdleState;
};

extern "C" __declspec(dllexport) int GetBaseSpeed() {
  int speed = 0;
  SYSTEM_INFO sysInfo;
  GetSystemInfo(&sysInfo);
  int numProcessors = sysInfo.dwNumberOfProcessors;
  std::vector<PROCESSOR_POWER_INFORMATION> info(numProcessors);
  //std::cout << info.size() << std::endl;

  if(CallNtPowerInformation(ProcessorInformation, NULL, 0, &info[0], sizeof(PROCESSOR_POWER_INFORMATION) * numProcessors) == 0) {
    speed = info[0].MaxMhz;
    //std::cout << "Base Speed: " << info[0].MaxMhz << " MHz" << std::endl;
  }
  return speed;
}

extern "C" __declspec(dllexport) int GetSocketNum() {
  int socketNum = 0;
  DWORD returnLength = 0;
  // Get required buffer size
  GetLogicalProcessorInformation(NULL, &returnLength);

  std::vector<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> buffer(returnLength / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));

  if(GetLogicalProcessorInformation(buffer.data(), &returnLength)) {
    for(const auto& info : buffer) {
      if(info.Relationship == RelationProcessorPackage) {
        socketNum++;
      }
    }
  }
  return socketNum;
}

extern "C" __declspec(dllexport) InstructionSetStruct GetInstructionSetStruct() {
  InstructionSetStruct data = {
    InstructionSet::_3DNOW(),
    InstructionSet::_3DNOWEXT(),
    InstructionSet::ABM(),
    InstructionSet::ADX(),
    InstructionSet::AES(),
    InstructionSet::AVX(),
    InstructionSet::AVX2(),
    InstructionSet::AVX512CD(),
    InstructionSet::AVX512ER(),
    InstructionSet::AVX512F(),
    InstructionSet::AVX512PF(),
    InstructionSet::BMI1(),
    InstructionSet::BMI2(),
    InstructionSet::CLFSH(),
    InstructionSet::CMPXCHG16B(),
    InstructionSet::CX8(),
    InstructionSet::ERMS(),
    InstructionSet::F16C(),
    InstructionSet::FMA(),
    InstructionSet::FSGSBASE(),
    InstructionSet::FXSR(),
    InstructionSet::HLE(),
    InstructionSet::INVPCID(),
    InstructionSet::LAHF(),
    InstructionSet::LZCNT(),
    InstructionSet::MMX(),
    InstructionSet::MMXEXT(),
    InstructionSet::MONITOR(),
    InstructionSet::MOVBE(),
    InstructionSet::MSR(),
    InstructionSet::OSXSAVE(),
    InstructionSet::PCLMULQDQ(),
    InstructionSet::POPCNT(),
    InstructionSet::PREFETCHWT1(),
    InstructionSet::RDRAND(),
    InstructionSet::RDSEED(),
    InstructionSet::RDTSCP(),
    InstructionSet::RTM(),
    InstructionSet::SEP(),
    InstructionSet::SHA(),
    InstructionSet::SSE(),
    InstructionSet::SSE2(),
    InstructionSet::SSE3(),
    InstructionSet::SSE41(),
    InstructionSet::SSE42(),
    InstructionSet::SSE4a(),
    InstructionSet::SSSE3(),
    InstructionSet::SYSCALL(),
    InstructionSet::TBM(),
    InstructionSet::XOP(),
    InstructionSet::XSAVE(),
  };

  return data;
}

//extern "C" __declspec(dllexport) void GetLogicalProcessorInfo() {
//  DWORD length = 0;
//  GetLogicalProcessorInformation(nullptr, &length);
//  std::vector<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> buffer(length / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
//  if(GetLogicalProcessorInformation(buffer.data(), &length)) {
//    DWORD physicalCores = 0;
//    for(const auto& info : buffer) {
//      if(info.Relationship == RelationProcessorCore) {
//        physicalCores++;
//      }
//    }
//    std::cout << "Physical cores: " << physicalCores << std::endl;
//  }
//}

extern "C" __declspec(dllexport) UINT32 GetPhysicalCoreCount() {
  DWORD length = 0;
  GetLogicalProcessorInformation(nullptr, &length);
  std::vector<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> buffer(length / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
  if(GetLogicalProcessorInformation(buffer.data(), &length)) {
    DWORD physicalCores = 0;
    for(const auto& info : buffer) {
      if(info.Relationship == RelationProcessorCore) {
        physicalCores++;
      }
    }
    return physicalCores;
  }
  // If the function fails, return 0
  return 0;
}

extern "C" __declspec(dllexport) UINT32 GetLogicalCoreCount() {
  DWORD count = GetActiveProcessorCount(ALL_PROCESSOR_GROUPS);
  return count;
}

extern "C" __declspec(dllexport) void GetCacheInfo() {
  DWORD bufferSize = 0;
  GetLogicalProcessorInformation(nullptr, &bufferSize);

  std::vector<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> buffer(bufferSize / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
  if(!GetLogicalProcessorInformation(buffer.data(), &bufferSize)) {
    return;
  }

  for(const auto& info : buffer) {
    if(info.Relationship == RelationCache) {
      const CACHE_DESCRIPTOR& cache = info.Cache;
      std::cout << "L" << (int) cache.Level << " Cache:" << std::endl;
      std::cout << "  Size: " << cache.Size << " bytes" << std::endl;
      std::cout << "  Line Size: " << cache.LineSize << " bytes" << std::endl;
      std::cout << "  Type: " << (cache.Type == CacheUnified ? "Unified" :
        cache.Type == CacheData ? "Data" : "Instruction") << std::endl;
    }
  }
}