using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace Crystal.CpuModule.Models;

/// <summary>
/// The module's data source: exposes the two CPU information streams a view model
/// consumes — static <see cref="Specs"/> (emitted once) and live <see cref="Sensors"/>
/// (re-sampled on a cadence). Both carry the neutral <see cref="ISystemCpuInfo"/> aggregate.
/// </summary>
public interface ICpuModel {
  IObservable<ISystemCpuInfo> Specs { get; }
  IObservable<ISystemCpuInfo> Sensors { get; }
}
