using System;
using Crystal.Ioc;
using Unity;

namespace Crystal.Unity
{
	public abstract class CrystalApplication : CrystalApplicationBase
	{
    /// <summary>
    /// Create a new <see cref="UnityContainerExtension"/> used by Prism.
    /// </summary>
    /// <returns>A new <see cref="UnityContainerExtension"/>.</returns>
    protected override IContainerExtension CreateContainerExtension()
    {
      return new UnityContainerExtension();
    }

    /// <summary>
    /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
    /// root exceptions by the <see cref="ExceptionExtensions"/>.
    /// </summary>
    protected override void RegisterFrameworkExceptionTypes()
    {
      ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ResolutionFailedException));
    }
  }
}
