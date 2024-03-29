﻿namespace Crystal;

/// <summary>
/// Base application class that provides a basic initialization sequence.
/// </summary>
/// <remarks>
/// This class must be overridden to provide application specific configuration.
/// </remarks>
public abstract class CrystalApplication : CrystalApplicationBase
{
  /// <summary>
  /// Create <see cref="Rules"/> to alter behavior of <see cref="IContainer"/>.
  /// </summary>
  /// <returns>An instance of <see cref="Rules"/></returns>
  protected virtual Rules CreateContainerRules() 
    => ContainerExtension.DefaultRules;

  /// <summary>
  /// Create a new <see cref="ContainerExtension"/> used by Crystal.
  /// </summary>
  /// <returns>A new <see cref="ContainerExtension"/>.</returns>
  protected override IContainerExtension CreateContainerExtension() 
    => new ContainerExtension(new Container(CreateContainerRules()));

  /// <summary>
  /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
  /// root exceptions by the <see cref="ExceptionExtensions"/>.
  /// </summary>
  protected override void RegisterFrameworkExceptionTypes() 
    => ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ContainerException));
}