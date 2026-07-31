using System.Collections.Generic;

namespace Crystal.Telemetry.Hardware.Motherboard.Lpc.EC;

/// <summary>
/// <see cref="EmbeddedController" /> implementation for Windows-based embedded controllers.
/// </summary>
public class WindowsEmbeddedController : EmbeddedController {
  /// <summary>
  /// Initializes a new instance of the <see cref="WindowsEmbeddedController" /> class.
  /// </summary>
  /// <param name="sources">The embedded controller sources to read.</param>
  /// <param name="settings">Additional settings passed by the <see cref="IComputer" />.</param>
  public WindowsEmbeddedController(IEnumerable<EmbeddedControllerSource> sources, ISettings settings) : base(sources, settings) { }

  /// <inheritdoc />
  protected override IEmbeddedControllerIO AcquireIOInterface() {
    return new WindowsEmbeddedControllerIO();
  }
}