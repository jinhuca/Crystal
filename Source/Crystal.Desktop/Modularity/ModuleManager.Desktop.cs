namespace Crystal;

/// <summary>
/// Component responsible for coordinating the modules' type loading and module initialization process. 
/// </summary>
public partial class ModuleManager
{
  /// <summary>
  /// Returns the list of registered <see cref="IModuleTypeLoader"/> instances that will be 
  /// used to load the types of modules. 
  /// </summary>
  /// <value>The module type loaders.</value>
  public virtual IEnumerable<IModuleTypeLoader> ModuleTypeLoaders
  {
    get => typeLoaders ??= new List<IModuleTypeLoader> { new FileModuleTypeLoader() };
    set => typeLoaders = value;
  }

}