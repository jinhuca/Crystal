using System.Windows.Controls.Primitives;

namespace Crystal;

internal static class CrystalInitializationExtensions
{
  internal static void ConfigureViewModelLocator()
  {
    ViewModelLocationProvider.SetDefaultViewModelFactory((_, type) => ContainerLocator.Container.Resolve(type));
  }

  internal static void RegisterRequiredTypes(this IContainerRegistry containerRegistry, IModuleCatalog moduleCatalog)
  {
    containerRegistry.RegisterInstance(moduleCatalog);
    containerRegistry.RegisterSingleton<IDialogService, DialogService>();
    containerRegistry.RegisterSingleton<IModuleInitializer, ModuleInitializer>();
    containerRegistry.RegisterSingleton<IModuleManager, ModuleManager>();
    containerRegistry.RegisterSingleton<RegionAdapterMappings>();
    containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
    containerRegistry.RegisterSingleton<IRegionNavigationContentLoader, RegionNavigationContentLoader>();
    containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
    containerRegistry.RegisterSingleton<IRegionViewRegistry, RegionViewRegistry>();
    containerRegistry.RegisterSingleton<IRegionBehaviorFactory, RegionBehaviorFactory>();
    containerRegistry.Register<IRegionNavigationJournalEntry, RegionNavigationJournalEntry>();
    containerRegistry.Register<IRegionNavigationJournal, RegionNavigationJournal>();
    containerRegistry.Register<IRegionNavigationService, RegionNavigationService>();
    containerRegistry.Register<IDialogWindow, DialogWindow>(); //default dialog host
  }

  internal static void RegisterDefaultRegionBehaviors(this IRegionBehaviorFactory regionBehaviors)
  {
    regionBehaviors.AddIfMissing<BindRegionContextToDependencyObjectBehavior>(BindRegionContextToDependencyObjectBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<RegionActiveAwareBehavior>(RegionActiveAwareBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<SyncRegionContextWithHostBehavior>(SyncRegionContextWithHostBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<RegionManagerRegistrationBehavior>(RegionManagerRegistrationBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<RegionMemberLifetimeBehavior>(RegionMemberLifetimeBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<ClearChildViewsRegionBehavior>(ClearChildViewsRegionBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<AutoPopulateRegionBehavior>(AutoPopulateRegionBehavior.BehaviorKey);
    regionBehaviors.AddIfMissing<DestructibleRegionBehavior>(DestructibleRegionBehavior.BehaviorKey);
  }

  internal static void RegisterDefaultRegionAdapterMappings(this RegionAdapterMappings regionAdapterMappings)
  {
    regionAdapterMappings.RegisterMapping<Selector, SelectorRegionAdapter>();
    regionAdapterMappings.RegisterMapping<ItemsControl, ItemsControlRegionAdapter>();
    regionAdapterMappings.RegisterMapping<ContentControl, ContentControlRegionAdapter>();
  }

  internal static void RunModuleManager(IContainerProvider containerProvider)
  {
    var manager = containerProvider.Resolve<IModuleManager>();
    manager.Run();
  }
}