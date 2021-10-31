using System;

namespace Crystal
{
	/// <summary>
	/// Specifies that the current module has a dependency on another module. This attribute should be used on classes that implement <see cref="IModule"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ModuleDependencyAttribute : Attribute
	{

		/// <summary>
		/// Initializes a new instance of <see cref="ModuleDependencyAttribute"/>.
		/// </summary>
		/// <param name="moduleName">The name of the module that this module is dependent upon.</param>
		public ModuleDependencyAttribute(string moduleName)
		{
			ModuleName = moduleName;
		}

		/// <summary>
		/// Gets the name of the module that this module is dependent upon.
		/// </summary>
		/// <value>The name of the module that this module is dependent upon.</value>
		public string ModuleName { get; }
	}
}
