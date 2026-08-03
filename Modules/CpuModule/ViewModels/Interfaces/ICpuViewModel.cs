using System;
using System.Collections.Generic;
using System.Text;

namespace CpuModule.ViewModels.Interfaces; 
public interface ICpuViewModel {
  ICpuSpecsViewModel SpecsViewModel { get; }
  ICpuSensorViewModel SensorsViewModel { get; }
}
