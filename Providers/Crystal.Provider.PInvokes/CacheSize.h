#pragma once
#include <Windows.h>
#include <vector>

struct CacheSize {
  int L1_cache_size;
  int L1_cache_line_size;
  int L2_cache_size;
  int L2_cache_line_size;
  int L3_cache_size;
  int L3_cache_line_size;
};

extern "C" __declspec(dllexport) CacheSize __stdcall GetCacheSize() {
  CacheSize cache_info = {0, 0, 0, 0, 0, 0};
  DWORD buffer_size = 0;
  // First call to determine required buffer size
  GetLogicalProcessorInformation(nullptr, &buffer_size);
  if(buffer_size == 0) {
    return cache_info;
  }
  std::vector<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> buffer(buffer_size / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
  if(GetLogicalProcessorInformation(buffer.data(), &buffer_size)) {
    for(const auto& info : buffer) {
      if(info.Relationship == RelationCache) {
        const CACHE_DESCRIPTOR& cache = info.Cache;
        switch(cache.Level) {
          case 1:
            cache_info.L1_cache_size += cache.Size;
            cache_info.L1_cache_line_size = cache.LineSize;
            break;
          case 2:
            cache_info.L2_cache_size += cache.Size;
            cache_info.L2_cache_line_size = cache.LineSize;
            break;
          case 3:
            cache_info.L3_cache_size += cache.Size;
            cache_info.L3_cache_line_size = cache.LineSize;
            break;
        }
      }
    }
  }
  return cache_info;
}