using Crystal.BenchmarkModule.ViewModels;
using Crystal.BenchmarkModule.Views;
using Crystal.Service.Benchmark;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

namespace Crystal.BenchmarkModule;

/// <summary>
/// Prism module for the benchmark suite. Registers the pure-C# benchmark engine
/// (<see cref="IBenchmarkCatalog"/> + <see cref="BenchmarkRunner"/>) and the dashboard VM, and wires
/// the <see cref="BenchmarkDetailView"/> to it. Unlike the sensor modules it injects nothing into the
/// dashboard: the benchmark surface is detail-window only, opened on demand from the shell title bar
/// and the Processes detail toolbar, so <see cref="OnInitialized"/> registers no dashboard region.
/// </summary>
public class BenchmarkModule : IModule {
  public void RegisterTypes(IContainerRegistry containerRegistry) {
    // The catalog just constructs the fixed set of suites; the runner is stateless (per-run
    // cancellation lives on the VM). Singletons are fine — one of each for the app.
    containerRegistry.RegisterSingleton<IBenchmarkCatalog, BenchmarkCatalog>();
    containerRegistry.RegisterSingleton<BenchmarkRunner>();

    // One VM per view: DetailWindowService disposes the VM (cancelling any in-flight run) when the
    // window closes, so a fresh instance must back each newly opened window.
    containerRegistry.Register<BenchmarkDashboardViewModel>();

    ViewModelLocationProvider.Register<BenchmarkDetailView>(
        () => ContainerLocator.Container.Resolve<BenchmarkDashboardViewModel>());
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    // Detail-window only — no dashboard tile to inject.
  }
}
