namespace Crystal;

/// <summary>
/// Represents a catalog created from a directory on disk.
/// </summary>
/// <remarks>
/// The directory catalog will scan the contents of a directory, locating classes that implement
/// <see cref="IModule"/> and add them to the catalog based on contents in their associated <see cref="ModuleAttribute"/>.
/// Assemblies are loaded into a new application domain with ReflectionOnlyLoad.  The application domain is destroyed
/// once the assemblies have been discovered.
/// The directory catalog does not continue to monitor the directory after it has created the initialize catalog.
/// </remarks>
public class DirectoryModuleCatalog : ModuleCatalog
{
  /// <summary>
  /// Directory containing modules to search for.
  /// </summary>
  public string ModulePath { get; set; }

  /// <summary>
  /// Drives the main logic of building the child domain and searching for the assemblies.
  /// </summary>
  protected override void InnerLoad()
  {
    if (IsNullOrEmpty(ModulePath))
    {
      throw new InvalidOperationException(ModulePathCannotBeNullOrEmpty);
    }

    if (!Directory.Exists(ModulePath))
    {
      throw new InvalidOperationException(Format(DirectoryNotFound, ModulePath));
    }

    AppDomain childDomain = AppDomain.CurrentDomain;

    try
    {
      List<string> loadedAssemblies = new();

      var assemblies = (
        from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
        where !(assembly is System.Reflection.Emit.AssemblyBuilder)
              && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
              && !IsNullOrEmpty(assembly.Location)
        select assembly.Location);

      loadedAssemblies.AddRange(assemblies);
      var loaderType = typeof(InnerModuleInfoLoader);
      if (loaderType.Assembly != null)
      {
        var loader = (InnerModuleInfoLoader)childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName)?.Unwrap();
        Items.AddRange(loader?.GetModuleInfos(ModulePath));
      }
    }
    catch (Exception ex)
    {
      throw new Exception("There was an error loading assemblies.", ex);
    }
  }

  private class InnerModuleInfoLoader : MarshalByRefObject
  {
    internal ModuleInfo[] GetModuleInfos(string path)
    {
      DirectoryInfo directory = new(path);
      Assembly ResolveEventHandler(object sender, ResolveEventArgs args) => OnReflectionOnlyResolve(args, directory);
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveEventHandler;
      Assembly moduleReflectionOnlyAssembly = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.FullName == typeof(IModule).Assembly.FullName);
      var IModuleType = moduleReflectionOnlyAssembly.GetType(typeof(IModule).FullName ?? Empty);
      IEnumerable<ModuleInfo> modules = GetNotAlreadyLoadedModuleInfos(directory, IModuleType);
      var array = modules.ToArray();
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= ResolveEventHandler;
      return array;
    }

    private static IEnumerable<ModuleInfo> GetNotAlreadyLoadedModuleInfos(DirectoryInfo directory, Type IModuleType)
    {
      List<Assembly> validAssemblies = new();
      Assembly[] alreadyLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).ToArray();

      var fileInfos = directory.GetFiles("*.dll")
        .Where(file => alreadyLoadedAssemblies.FirstOrDefault(assembly
          => Compare(Path.GetFileName(assembly.Location), file.Name, StringComparison.OrdinalIgnoreCase) == 0) == null).ToList();

      foreach (FileInfo fileInfo in fileInfos)
      {
        try
        {
          validAssemblies.Add(Assembly.LoadFrom(fileInfo.FullName));
        }
        catch (BadImageFormatException)
        {
          // skip non-.NET Dlls
        }
      }

      return validAssemblies.SelectMany(assembly => assembly
        .GetExportedTypes()
        .Where(IModuleType.IsAssignableFrom)
        .Where(t => t != IModuleType)
        .Where(t => !t.IsAbstract)
        .Select(CreateModuleInfo));
    }

    private static Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
    {
      Assembly loadedAssembly = AppDomain
        .CurrentDomain
        .ReflectionOnlyGetAssemblies()
        .FirstOrDefault(asm => String.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

      if (loadedAssembly != null)
      {
        return loadedAssembly;
      }
      AssemblyName assemblyName = new(args.Name);
      string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");
      return File.Exists(dependentAssemblyFilename) ? Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename) : Assembly.ReflectionOnlyLoad(args.Name);
    }

    private static ModuleInfo CreateModuleInfo(Type type)
    {
      string moduleName = type.Name;
      List<string> dependsOn = new();
      bool onDemand = false;
      var moduleAttribute = CustomAttributeData
        .GetCustomAttributes(type).FirstOrDefault(cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

      if (moduleAttribute != null)
      {
        foreach (var argument in moduleAttribute.NamedArguments)
        {
          string argumentName = argument.MemberInfo.Name;
          switch (argumentName)
          {
            case "ModuleName":
              moduleName = (string)argument.TypedValue.Value;
              break;

            case "OnDemand":
              onDemand = (bool)argument.TypedValue.Value;
              break;

            case "StartupLoaded":
              onDemand = !((bool)argument.TypedValue.Value);
              break;
          }
        }
      }

      var moduleDependencyAttributes = CustomAttributeData
        .GetCustomAttributes(type)
        .Where(cad => cad.Constructor.DeclaringType?.FullName == typeof(ModuleDependencyAttribute).FullName);

      foreach (var cad in moduleDependencyAttributes)
      {
        dependsOn.Add(item: (string)cad.ConstructorArguments[0].Value);
      }

      ModuleInfo moduleInfo = new(moduleName, type.AssemblyQualifiedName)
      {
        InitializationMode = onDemand ? InitializationMode.OnDemand : InitializationMode.WhenAvailable,
        Ref = type.Assembly.Location
      };
      moduleInfo.DependsOn.AddRange(dependsOn);
      return moduleInfo;
    }
  }
}