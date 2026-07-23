using System.ComponentModel.DataAnnotations;

namespace BiosModule;

public class BiosModule(IRegionManager regionManager) : IModule {
  [Required]
  private readonly IRegionManager _regionManager = regionManager;

  public void OnInitialized(IContainerProvider containerProvider) {
    throw new NotImplementedException();
  }

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    throw new NotImplementedException();
  }
}
