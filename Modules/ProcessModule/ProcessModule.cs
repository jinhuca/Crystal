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

    // ProcessMonitor owns the poll cadence and the cross-poll CPU-time baseline, so it must be a
    // singleton. Built via a factory: its optional TimeSpan?/IScheduler? params can't be resolved
    // by the container, and we want the default 1-second cadence.
    containerRegistry.RegisterSingleton<ProcessMonitor>(
        cp => new ProcessMonitor(cp.Resolve<IWmiHardwareProvider>(), cp.Resolve<IProcessEtwSource>()));
    containerRegistry.RegisterSingleton<IProcessModel, ProcessModel>();

    // One VM instance per view; the tile is the only consumer today.
    containerRegistry.Register<ProcessListViewModel>();

    ViewModelLocationProvider.Register<ProcessSummaryView>(
        () => ContainerLocator.Container.Resolve<ProcessListViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    _regionManager.RegisterViewWithRegion(RegionNames.ProcessesRegionName, typeof(ProcessSummaryView));
  }
}
