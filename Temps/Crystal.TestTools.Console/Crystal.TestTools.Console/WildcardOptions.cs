using System;

namespace Crystal.TestTools.Console
{
	/// <summary>
	/// Provides enumerated values to use to set wildcard pattern
	/// matching options.
	/// </summary>
	[Flags]
	public enum WildcardOptions
	{
		/// <summary>
		/// Indicates that no special processing is required.
		/// </summary>
		None = 0,

		/// <summary>
		/// Specifies that the wildcard pattern is compiled to an assembly.
		/// This yields faster execution but increases startup time.
		/// </summary>
		Compiled = 1,

		/// <summary>
		/// Specifies case-insensitive matching.
		/// </summary>
		IgnoreCase = 2,

		/// <summary>
		/// Specifies culture-invariant matching.
		/// </summary>
		CultureInvariant = 4
	};
}
