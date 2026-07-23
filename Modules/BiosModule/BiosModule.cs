using System.ComponentModel.DataAnnotations;

namespace BiosModule;

public class BiosModule : IModule {
  [Required]
  private readonly IRegionManager _regionManager;

  public BiosModule(IRegionManager regionManager) {
    
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    throw new NotImplementedException();
  }

  public void RegisterTypes(IContainerRegistry containerRegistry) {
    throw new NotImplementedException();
  }
}
