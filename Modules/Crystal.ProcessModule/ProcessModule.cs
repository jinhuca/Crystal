using Crystal.Infrastructure.Constants;
using Crystal.ProcessModule.Models;
using Crystal.ProcessModule.ViewModels;
using Crystal.ProcessModule.Views;
using Crystal.Provider.Etw;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Gpu;
using Crystal.Service.Process;

namespace Crystal.ProcessModule;

/// <summary>
/// Prism module for the process list. Registers the provider→monitor→model→view-model chain and
/// injects the compact <see cref="ProcessSummaryView"/> (live process/thread/handle totals) into the
/// dashboard's Processes region; double-clicking that tile opens the Task Manager-style
/// <see cref="ProcessDetailView"/> in its own window.
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

    // Per-process GPU% from the GPU Engine performance counters — Task Manager's own source, which
    // (unlike the ETW DMA-packet estimate) captures compute/video workloads. Singleton because it
    // owns a PDH query handle for the app lifetime; stays inert if the query can't be opened.
    containerRegistry.RegisterSingleton<GpuProcessUsageSampler>();

    // ProcessMonitor owns the poll cadence and the cross-poll CPU-time baseline, so it must be a
    // singleton. Built via a factory: its optional TimeSpan?/IScheduler? params can't be resolved
    // by the container, and we want the default 1-second cadence.
    containerRegistry.RegisterSingleton<ProcessMonitor>(
        cp => new ProcessMonitor(cp.Resolve<IWmiHardwareProvider>(), cp.Resolve<EtwRateBroadcaster>(),
            cp.Resolve<GpuProcessUsageSampler>()));
    containerRegistry.RegisterSingleton<IProcessModel, ProcessModel>();

    // System-wide process/thread/handle totals for the summary header. Singleton so its
    // ref-counted poll timer is shared; built via a factory because its optional
    // TimeSpan?/IScheduler? ctor params can't be resolved by the container (default 1s cadence).
    containerRegistry.RegisterSingleton<SystemStatsMonitor>(_ => new SystemStatsMonitor());

    // Shell-icon extractor for the process list; singleton so its per-path icon cache is shared and
    // built once. Extraction runs off the UI thread and returns frozen images.
    containerRegistry.RegisterSingleton<ProcessIconProvider>();

    // Terminates / launches processes for the End task / Run new task actions. Stateless, so a
    // singleton is fine.
    containerRegistry.RegisterSingleton<IProcessController, ProcessController>();

    // Records the selected process's per-poll readings to a CSV. Holds a file handle only while a
    // recording is active; one recording at a time, so a singleton (one live file) is correct.
    containerRegistry.RegisterSingleton<IProcessRecorder, ProcessRecorder>();

    // One VM instance per view; the detail window is the only consumer today. Built via a factory
    // because its optional Func<DateTimeOffset>? clock param isn't injected by Unity (it defaults to
    // the system clock for the live export timestamp).
    containerRegistry.Register<ProcessListViewModel>(
        cp => new ProcessListViewModel(cp.Resolve<IProcessModel>(), cp.Resolve<SystemStatsMonitor>(),
            cp.Resolve<ProcessIconProvider>(), controller: cp.Resolve<IProcessController>(),
            recorder: cp.Resolve<IProcessRecorder>(), gpuMonitor: cp.Resolve<GpuMonitor>()));

    // Lightweight VM for the compact dashboard tile: process/thread/handle totals only, no list.
    containerRegistry.Register<ProcessSummaryViewModel>();

    // The compact tile shows the totals; the full master-detail list is the detail-window surface.
    ViewModelLocationProvider.Register<ProcessSummaryView>(
        () => ContainerLocator.Container.Resolve<ProcessSummaryViewModel>());
    ViewModelLocationProvider.Register<ProcessDetailView>(
        () => ContainerLocator.Container.Resolve<ProcessListViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // The compact tile only reads the lightweight system-stats totals, so it renders immediately —
    // no loading spinner and, unlike the sensor tiles, no eager warm-up. The heavy process list
    // (ETW session, per-process enumeration) stays on-demand: it spins up only when the detail
    // window is opened from a double-click on this tile.
    _regionManager.RegisterViewWithRegion(
        RegionNames.ProcessesRegionName, typeof(ProcessSummaryView));
  }
}
