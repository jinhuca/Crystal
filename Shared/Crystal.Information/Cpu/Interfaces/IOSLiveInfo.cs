using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Information.Cpu.Interfaces; 
public interface IOSLiveInfo {
  int ProcessNum { get; set; }
  int ThreadsNum { get; set; }
  int HandlesNum { get; set; }
  TimeSpan UpTime { get; set; }
}
