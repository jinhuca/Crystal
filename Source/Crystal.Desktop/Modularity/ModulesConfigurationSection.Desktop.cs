namespace Crystal;

/// <summary>
/// A <see cref="ConfigurationSection"/> for module configuration.
/// </summary>
public class ModulesConfigurationSection : ConfigurationSection
{
  /// <summary>
  /// Gets or sets the collection of modules configuration.
  /// </summary>
  /// <value>A <see cref="ModuleConfigurationElementCollection"/> of <see cref="ModuleConfigurationElement"/>.</value>
  [ConfigurationProperty("", IsDefaultCollection = true, IsKey = false)]
  public ModuleConfigurationElementCollection Modules
  {
    get => (ModuleConfigurationElementCollection)base[""];
    set => base[""] = value;
  }
}