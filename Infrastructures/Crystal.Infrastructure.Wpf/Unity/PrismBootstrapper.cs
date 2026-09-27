using Unity;

namespace Crystal.Infrastructure.Wpf.Unity;

/// <summary>
/// Base bootstrapper class that uses <see cref="UnityContainerExtension"/> as it's container.
/// </summary>
public abstract class PrismBootstrapper : PrismBootstrapperBase {
  /// <summary>
  /// Create a new <see cref="UnityContainerExtension"/> used by Prism.
  /// </summary>
  /// <returns>A new <see cref="UnityContainerExtension"/>.</returns>
  protected override IContainerExtension CreateContainerExtension() {
    return new UnityContainerExtension();
  }

  /// <summary>
  /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
  /// root exceptions by the <c>ExceptionExtensions</c>.
  /// </summary>
  protected override void RegisterFrameworkExceptionTypes() {
    ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ResolutionFailedException));
  }
}
