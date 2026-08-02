namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC;

/// <summary>
/// Reads a value from the given register of the embedded controller.
/// </summary>
/// <param name="ecIO">The embedded controller I/O interface used to read the register.</param>
/// <param name="register">The register to read.</param>
/// <returns>The value read from the register.</returns>
public delegate float EmbeddedControllerReader(IEmbeddedControllerIO ecIO, ushort register);