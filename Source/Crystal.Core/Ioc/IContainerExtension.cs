﻿namespace Crystal
{
	/// <summary>
	/// A strongly typed container extension
	/// </summary>
	/// <typeparam name="TContainer">The underlying root container</typeparam>
	public interface IContainerExtension<out TContainer> : IContainerExtension
	{
		/// <summary>
		/// The instance of the wrapped container
		/// </summary>
		TContainer Instance { get; }
	}

	/// <summary>
	/// A generic abstraction for what Crystal expects from a container
	/// </summary>
	public interface IContainerExtension : IContainerProvider, IContainerRegistry
	{
		/// <summary>
		/// Used to perform any final steps for configuring the extension that may be required by the container.
		/// </summary>
		void FinalizeExtension();
	}
}
