using Crystal.Controls.Loading;
using Crystal.Infrastructure.Constants;
using Crystal.Provider.Etw;
using Crystal.Provider.Mmi.MmiEngine;
using ProcessModule.Models;
using ProcessModule.ViewModels;
using ProcessModule.Views;

namespace ProcessModule;

/// <summary>
/// Prism module for the process list. Registers the provider→monitor→model→view-model chain and
/// injects the Task Manager-style <see cref="ProcessSummaryView"/> into the dashboard's Processes
/// region. Unlike the sensor tiles there is no detail view — the list is the full surface.
/// </summary>
public class ProcessModule(IRegionManager regionManager) : IModule {
  private readonly IRegionManager _regionManager = regionManager;

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<IWmiHardwareProvider, WmiHardwareProvider>();

    // The ETW reader opens a kernel trace session in its constructor (needs elevation) and owns it
    // for the app lifetime, so it must be a singleton. If the session can't start it stays inert.
    containerRegistry.RegisterSingleton<IProcessEtwSource, ProcessEtwReader>();

    // The broadcaster owns the single SnapshotRates() poll and multicasts it, so the process list
    // and the network top-talkers view share one destructive snapshot instead of stealing each
    // other's interval. Singleton; built via a factory for its optional TimeSpan?/IScheduler? params.
    containerRegistry.RegisterSingleton<EtwRateBroadcaster>(
        cp => new EtwRateBroadcaster(cp.Resolve<IProcessEtwSource>()));

    // ProcessMonitor owns the poll cadence and the cross-poll CPU-time baseline, so it must be a
    // singleton. Built via a factory: its optional TimeSpan?/IScheduler? params can't be resolved
    // by the container, and we want the default 1-second cadence.
    containerRegistry.RegisterSingleton<ProcessMonitor>(
        cp => new ProcessMonitor(cp.Resolve<IWmiHardwareProvider>(), cp.Resolve<EtwRateBroadcaster>()));
    containerRegistry.RegisterSingleton<IProcessModel, ProcessModel>();

    // System-wide process/thread/handle totals for the summary header. Singleton so its
    // ref-counted poll timer is shared; built via a factory because its optional
    // TimeSpan?/IScheduler? ctor params can't be resolved by the container (default 1s cadence).
    containerRegistry.RegisterSingleton<SystemStatsMonitor>(_ => new SystemStatsMonitor());

    // Shell-icon extractor for the process list; singleton so its per-path icon cache is shared and
    // built once. Extraction runs off the UI thread and returns frozen images.
    containerRegistry.RegisterSingleton<ProcessIconProvider>();

    // One VM instance per view; the tile is the only consumer today. Built via a factory because
    // its optional Func<DateTimeOffset>? clock param isn't injected by Unity (it defaults to the
    // system clock for the live export timestamp).
    containerRegistry.Register<ProcessListViewModel>(
        cp => new ProcessListViewModel(cp.Resolve<IProcessModel>(), cp.Resolve<SystemStatsMonitor>(),
            cp.Resolve<ProcessIconProvider>()));

    ViewModelLocationProvider.Register<ProcessSummaryView>(
        () => ContainerLocator.Container.Resolve<ProcessListViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // Self-warming loading tile: spinner now, warm the model singleton off the UI thread, swap in
    // the real view when ready. See CpuModule for the rationale.
    _regionManager.RegisterViewWithRegion(RegionNames.ProcessesRegionName, () => {
      var host = new LoadingHost { Label = "Processes" };
      host.Begin(
          () => containerProvider.Resolve<IProcessModel>(),
          () => new ProcessSummaryView());
      return host;
    });
  }
}
