using System;
using Unity;

namespace Crystal.Unity
{
	public abstract class CrystalApplication : CrystalApplicationBase
	{
		/// <summary>
		/// Create a new <see cref="UnityContainerExtension"/> used by Crystal.
		/// </summary>
		/// <returns>A new <see cref="UnityContainerExtension"/>.</returns>
		protected override IContainerExtension CreateContainerExtension() 
			=> new UnityContainerExtension();

		/// <summary>
		/// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
		/// root exceptions by the <see cref="ExceptionExtensions"/>.
		/// </summary>
		protected override void RegisterFrameworkExceptionTypes() 
			=> ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ResolutionFailedException));
	}
}
