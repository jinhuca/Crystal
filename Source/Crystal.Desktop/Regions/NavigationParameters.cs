namespace Crystal;

/// <summary>
/// Represents Navigation parameters.
/// </summary>
/// <remarks>
/// This class can be used to to pass object parameters during Navigation.
/// </remarks>
public class NavigationParameters : ParametersBase
{
  /// <summary>
  /// Initializes a new instance of the <see cref="NavigationParameters"/> class.
  /// </summary>
  public NavigationParameters()
  { }

  /// <summary>
  /// Constructs a list of parameters
  /// </summary>
  /// <param name="query">Query string to be parsed</param>
  public NavigationParameters(string query) : base(query) { }
}