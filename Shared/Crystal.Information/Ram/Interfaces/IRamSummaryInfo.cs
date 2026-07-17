using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Information.Ram.Interfaces; 
internal interface IRamSummaryInfo {
  int? TotalRamInGB { get; set; }
  float? AvailableRamInGB { get; set; }
  float? UsagePercentage { get; set; }
}
