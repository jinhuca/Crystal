namespace Crystal;

/// <summary>
/// Defines the contract for the modules deployed in the application.
/// </summary>
public interface IModule
{
  /// <summary>
  /// Used to register types with the container that will be used by your application.
  /// </summary>
  void RegisterTypes(IContainerRegistry containerRegistry);

  /// <summary>
  /// Notifies the module that it has been initialized.
  /// </summary>
  void OnInitialized(IContainerProvider containerProvider);
}