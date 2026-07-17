namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a constraint which is capable to attach to or detach from <see cref="Viewport"/>, to which it is applied.
/// </summary>
public interface ISupportAttachToViewport {
  /// <summary>
  /// Attaches the specified viewport to a constraint.
  /// </summary>
  /// <param name="viewport">The viewport.</param>
  void Attach(Viewport2D viewport);

  /// <summary>
  /// Detaches the specified viewport from a constraint.
  /// </summary>
  /// <param name="viewport">The viewport.</param>
  void Detach(Viewport2D viewport);
}
