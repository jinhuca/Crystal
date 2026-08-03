using BiosModule.Models;
using CpuModule.Models;
using Crystal.Service.Sensors;
using GpuModule.Models;
using MemoryModule.Models;
using NetworkModule.Models;
using StorageModule.Models;

namespace Crystal.NewShell.Startup;

/// <summary>
/// Pre-warms the heavy singletons that would otherwise be resolved on the UI thread when the
/// dashboard tiles are first built. Each module's model opens a ring-0 LibreHardwareMonitor
/// session in its constructor, so resolving all of them synchronously froze the shell at startup.
/// <para>
/// Running <see cref="WarmUpAsync"/> off the UI thread builds every singleton up front (Unity
/// caches them), so subsequent tile construction just reuses the warmed instances. Progress is
/// reported per component so a loading screen can show what is happening.
/// </para>
/// </summary>
public sealed class StartupLoader {
  private readonly IContainerProvider _container;

  public StartupLoader(IContainerProvider container) => _container = container;

  /// <summary>
  /// The components warmed at startup, in load order. The factory resolves the module's
  /// registered singleton, forcing its constructor (and hardware session) to run now.
  /// </summary>
  private IReadOnlyList<(string Name, Action Warm)> Components => new (string, Action)[] {
    ("System sensors", () => _container.Resolve<SensorMonitor>()),
    ("CPU", () => _container.Resolve<ICpuModel>()),
    ("GPU", () => _container.Resolve<IGpuModel>()),
    ("Memory", () => _container.Resolve<IMemoryModel>()),
    ("Storage", () => _container.Resolve<IStorageModel>()),
    ("Network", () => _container.Resolve<INetworkModel>()),
    ("BIOS", () => _container.Resolve<IBiosModel>()),
  };

  /// <summary>Names of the components to warm, in load order — used to seed the loading checklist.</summary>
  public IReadOnlyList<string> ComponentNames => Components.Select(c => c.Name).ToList();

  /// <summary>
  /// Warms every component on a background thread, reporting progress before and after each one.
  /// A component that throws is reported as <see cref="StartupComponentState.Failed"/> and skipped
  /// so one missing sensor source can't block the whole app from starting.
  /// </summary>
  public Task WarmUpAsync(IProgress<StartupProgress> progress, CancellationToken ct = default) =>
      Task.Run(() => {
        var components = Components;
        var total = components.Count;
        var completed = 0;

        foreach (var (name, warm) in components) {
          ct.ThrowIfCancellationRequested();
          progress.Report(new StartupProgress(name, StartupComponentState.Loading, completed, total));

          var state = StartupComponentState.Completed;
          try {
            warm();
          }
          catch {
            // Warming is best-effort: the tile will still resolve (and surface its own error) later.
            state = StartupComponentState.Failed;
          }

          completed++;
          progress.Report(new StartupProgress(name, state, completed, total));
        }
      }, ct);
}
