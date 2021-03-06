using System;

namespace Crystal
{
	/// <summary>
	/// Helper class for parsing <see cref="Uri"/> instances.
	/// </summary>
	public static class UriParsingHelper
	{
		/// <summary>
		/// Gets the query part of <paramref name="uri"/>.
		/// </summary>
		/// <param name="uri">The Uri.</param>
		public static string GetQuery(Uri uri) => EnsureAbsolute(uri).Query;

		/// <summary>
		/// Gets the AbsolutePath part of <paramref name="uri"/>.
		/// </summary>
		/// <param name="uri">The Uri.</param>
		public static string GetAbsolutePath(Uri uri) => EnsureAbsolute(uri).AbsolutePath;

		/// <summary>
		/// Parses the query of <paramref name="uri"/> into a dictionary.
		/// </summary>
		/// <param name="uri">The URI.</param>
		public static NavigationParameters ParseQuery(Uri uri) => new(GetQuery(uri));

		private static Uri EnsureAbsolute(Uri uri)
		{
			if (uri.IsAbsoluteUri)
			{
				return uri;
			}

			if ((uri != null) && !uri.OriginalString.StartsWith("/", StringComparison.Ordinal))
			{
				return new Uri("http://localhost/" + uri, UriKind.Absolute);
			}
			return new Uri("http://localhost" + uri, UriKind.Absolute);
		}
	}
}
