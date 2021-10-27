using System;

namespace Crystal
{
	/// <summary>
	/// Defines that a view is synchronized with its parent view's Active state.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class SyncActiveStateAttribute : Attribute
	{
	}
}
