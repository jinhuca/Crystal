using System;

namespace Crystal
{
	/// <summary>
	/// Defines a Container Scope
	/// </summary>
	public interface IScopedProvider : IContainerProvider, IDisposable
	{
		/// <summary>
		/// Gets or Sets the IsAttached property.
		/// </summary>
		/// <remarks>
		/// Indicates that Crystal is tracking the scope
		/// </remarks>
		bool IsAttached { get; set; }
	}
}
