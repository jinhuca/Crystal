using System.Text;

namespace Crystal.TestTools.Console
{
	/// <summary>
	/// Translates a <see cref="WildcardPattern"/> into a DOS wildcard
	/// </summary>
	internal class WildcardPatternToDosWildcardParser : WildcardPatternParser
	{
		private readonly StringBuilder _result = new StringBuilder();

		protected override void AppendLiteralCharacter(char c)
		{
			_result.Append(c);
		}

		protected override void AppendAsterix()
		{
			_result.Append('*');
		}

		protected override void AppendQuestionMark()
		{
			_result.Append('?');
		}

		protected override void BeginBracketExpression()
		{
		}

		protected override void AppendLiteralCharacterToBracketExpression(char c)
		{
		}

		protected override void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange)
		{
		}

		protected override void EndBracketExpression()
		{
			_result.Append('?');
		}

		/// <summary>
		/// Converts <paramref name="wildcardPattern"/> into a DOS wildcard
		/// </summary>
		internal static string Parse(WildcardPattern wildcardPattern)
		{
			var parser = new WildcardPatternToDosWildcardParser();
			WildcardPatternParser.Parse(wildcardPattern, parser);
			return parser._result.ToString();
		}
	}
}
