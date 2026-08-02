#pragma once
#include <windows.h>
#include <stdio.h>
#include <vector>
#include <windows.h>
#include <pdh.h>
#pragma comment(lib, "pdh.lib")

extern "C" __declspec(dllexport) double __stdcall GetTotalCpuUtilization() {
  PDH_HQUERY cpuQuery;
  PDH_HCOUNTER cpuTotal;
  PdhOpenQuery(NULL, NULL, &cpuQuery);
  PdhAddEnglishCounter(cpuQuery, L"\\Processor(_Total)\\% Processor Time", NULL, &cpuTotal);
  PdhCollectQueryData(cpuQuery);
  Sleep(1000);
  PDH_FMT_COUNTERVALUE counterVal;
  PdhCollectQueryData(cpuQuery);
  PdhGetFormattedCounterValue(cpuTotal, PDH_FMT_DOUBLE, NULL, &counterVal);
  PdhCloseQuery(cpuQuery);
  return counterVal.doubleValue;
}

extern "C" __declspec(dllexport) double __stdcall GetCurrentCpuSpeed() {
  PDH_HQUERY cpuQuery;
  PDH_HCOUNTER cpuFrequency;
  PDH_FMT_COUNTERVALUE counterVal;

  // 1. Open a query
  if(PdhOpenQuery(NULL, NULL, &cpuQuery) != ERROR_SUCCESS) {
    return -1;
  }

  // 2. Add the Frequency counter
  // Note: Use "Processor Information" for modern Windows (10+)
  PdhAddCounter(cpuQuery, L"\\Processor Information(_Total)\\Processor Frequency", NULL, &cpuFrequency);

  // 3. Collect the initial data
  PdhCollectQueryData(cpuQuery);

  Sleep(1000);

  // 4. Retrieve and display the value
  if(PdhGetFormattedCounterValue(cpuFrequency, PDH_FMT_DOUBLE, NULL, &counterVal) == ERROR_SUCCESS) {
    std::cout << "Current CPU Speed: " << counterVal.doubleValue << " MHz" << std::endl;
  }

  // 5. Clean up
  PdhCloseQuery(cpuQuery);

  return counterVal.doubleValue;
}