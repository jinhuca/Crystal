using Crystal.Infrastructure.Core.Mvvm;

namespace Crystal.Infrastructure.Core.Common;

/// <summary>
/// An internal marker API used within Crystal.Infrastructure.Core to access the instance of the <see cref="IViewRegistry"/>
/// within a service where we do not want to publicly expose it but need access for Extension methods.
/// </summary>
public interface IRegistryAware {
  /// <summary>
  ///  The instance of the IViewRegistry
  /// </summary>
  IViewRegistry Registry { get; }
}
