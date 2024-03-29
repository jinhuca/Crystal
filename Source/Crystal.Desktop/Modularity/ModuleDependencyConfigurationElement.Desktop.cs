namespace Crystal;

/// <summary>
/// A <see cref="ConfigurationElement"/> for module dependencies.
/// </summary>
public class ModuleDependencyConfigurationElement : ConfigurationElement
{
  /// <summary>
  /// Initializes a new instance of <see cref="ModuleDependencyConfigurationElement"/>.
  /// </summary>
  public ModuleDependencyConfigurationElement()
  {
  }

  /// <summary>
  /// Initializes a new instance of <see cref="ModuleDependencyConfigurationElement"/>.
  /// </summary>
  /// <param name="moduleName">A module name.</param>
  public ModuleDependencyConfigurationElement(string moduleName)
  {
    base["moduleName"] = moduleName;
  }

  /// <summary>
  /// Gets or sets the name of a module another module depends on.
  /// </summary>
  /// <value>The name of a module another module depends on.</value>
  [ConfigurationProperty("moduleName", IsRequired = true, IsKey = true)]
  public string ModuleName
  {
    get => (string)base["moduleName"];
    set => base["moduleName"] = value;
  }
}