using System;
using System.Collections.Generic;
using static Crystal.Constants.StringConstants;
using static System.String;

namespace Crystal;

/// <summary>
/// Used by <see cref="IModuleInitializer"/> to get the load sequence
/// for the modules to load according to their dependencies.
/// </summary>
public class ModuleDependencySolver
{
  private readonly ListDictionary<string, string> dependencyMatrix = new();
  private readonly List<string> knownModules = new();

  /// <summary>
  /// Adds a module to the solver.
  /// </summary>
  /// <param name="name">The name that uniquely identifies the module.</param>
  public void AddModule(string name)
  {
    if (IsNullOrEmpty(name))
    {
      throw new ArgumentException(Format(StringCannotBeNullOrEmpty, nameof(name)));
    }

    AddToDependencyMatrix(name);
    AddToKnownModules(name);
  }

  /// <summary>
  /// Adds a module dependency between the modules specified by dependingModule and
  /// dependentModule.
  /// </summary>
  /// <param name="dependingModule">The name of the module with the dependency.</param>
  /// <param name="dependentModule">The name of the module dependingModule
  /// depends on.</param>
  public void AddDependency(string dependingModule, string dependentModule)
  {
    if (IsNullOrEmpty(dependingModule))
    {
      throw new ArgumentException(Format(StringCannotBeNullOrEmpty, nameof(dependentModule)));
    }

    if (IsNullOrEmpty(dependentModule))
    {
      throw new ArgumentException(Format(StringCannotBeNullOrEmpty, nameof(dependentModule)));
    }

    if (!knownModules.Contains(dependingModule))
    {
      throw new ArgumentException(Format(DependencyForUnknownModule, dependingModule));
    }

    AddToDependencyMatrix(dependentModule);
    dependencyMatrix.Add(dependentModule, dependingModule);
  }

  private void AddToDependencyMatrix(string module)
  {
    if (!dependencyMatrix.ContainsKey(module))
    {
      dependencyMatrix.Add(module);
    }
  }

  private void AddToKnownModules(string module)
  {
    if (!knownModules.Contains(module))
    {
      knownModules.Add(module);
    }
  }

  /// <summary>
  /// Calculates an ordered vector according to the defined dependencies.
  /// Non-dependent modules appears at the beginning of the resulting array.
  /// </summary>
  /// <returns>The resulting ordered list of modules.</returns>
  /// <exception cref="CyclicDependencyFoundException">This exception is thrown
  /// when a cycle is found in the defined dependency graph.</exception>
  public string[] Solve()
  {
    List<string> skip = new List<string>();
    while (skip.Count < dependencyMatrix.Count)
    {
      List<string> leaves = FindLeaves(skip);
      if (leaves.Count == 0 && skip.Count < dependencyMatrix.Count)
      {
        throw new CyclicDependencyFoundException(CyclicDependencyFound);
      }
      skip.AddRange(leaves);
    }
    skip.Reverse();

    if (skip.Count > knownModules.Count)
    {
      string moduleNames = FindMissingModules(skip);
      throw new ModularityException(moduleNames, Format(DependencyOnMissingModule, moduleNames));
    }

    return skip.ToArray();
  }

  private string FindMissingModules(List<string> skip)
  {
    string missingModules = "";

    foreach (string module in skip)
    {
      if (!knownModules.Contains(module))
      {
        missingModules += ", ";
        missingModules += module;
      }
    }

    return missingModules.Substring(2);
  }

  /// <summary>
  /// Gets the number of modules added to the solver.
  /// </summary>
  /// <value>The number of modules.</value>
  public int ModuleCount => dependencyMatrix.Count;

  private List<string> FindLeaves(List<string> skip)
  {
    List<string> result = new List<string>();

    foreach (string precedent in dependencyMatrix.Keys)
    {
      if (skip.Contains(precedent))
      {
        continue;
      }

      int count = 0;
      foreach (string dependent in dependencyMatrix[precedent])
      {
        if (skip.Contains(dependent))
        {
          continue;
        }
        count++;
      }
      if (count == 0)
      {
        result.Add(precedent);
      }
    }
    return result;
  }
}