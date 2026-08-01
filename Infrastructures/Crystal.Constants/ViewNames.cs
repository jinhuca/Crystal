using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Constants;

public static class ViewNames {
  public static string HomeViewName { get; set; } = "HomeContentView";
  public static string CpuViewName { get; set; } = "CpuSummaryView";
  public static string MemoryViewName { get; set; } = "MemorySummaryView";
  public static string StorageViewName { get; set; } = "StorageSummaryView";
  public static string WifiViewName { get; set; } = "WifiSummaryView";
  public static string GpuViewName { get; set; } = "GpuSummaryView";
  public static string FansViewName { get; set; } = "FansSummaryView";
}
