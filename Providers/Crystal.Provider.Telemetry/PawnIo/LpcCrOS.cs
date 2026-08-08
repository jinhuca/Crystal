using System;

namespace Crystal.Provider.Telemetry.PawnIo;

/// <summary>
/// Provides access to a ChromeOS embedded controller via the PawnIO LpcCrOSEC module.
/// </summary>
public class LpcCrOSEc {
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(LpcCrOSEc).Assembly, "Crystal.Provider.Telemetry.Resources.PawnIo.LpcCrOSEC.bin");

  /// <summary>
  /// Sends a command to the embedded controller and returns its response.
  /// </summary>
  /// <param name="version">The command version.</param>
  /// <param name="command">The command identifier.</param>
  /// <param name="outsize">The size of the outgoing data, in bytes.</param>
  /// <param name="insize">The size of the expected response data, in bytes.</param>
  /// <param name="data">The outgoing command data.</param>
  /// <returns>The response data returned by the embedded controller.</returns>
  public byte[] EcCommand(int version, int command, int outsize, int insize, byte[] data) {
    long[] inArray = new long[38];
    inArray[0] = version;
    inArray[1] = command;
    inArray[2] = outsize;
    inArray[3] = insize;

    // Start packing data into inArray at the 4th long (8 bytes)
    Buffer.BlockCopy(data, 0, inArray, 4 * 8, data.Length);

    long[] outArray = _pawnIO.Execute("ioctl_ec_command", inArray, 1 + (int)Math.Ceiling(insize / 8.0));
    if (outArray[0] < 0) {
      throw new Exception("EC returned error code " + -outArray[0]);
    }

    byte[] retArray = new byte[insize];
    // Unpack the data skipping the first long
    Buffer.BlockCopy(outArray, 8, retArray, 0, insize);
    return retArray;
  }

  /// <summary>
  /// Reads a block of bytes from the embedded controller memory map.
  /// </summary>
  /// <param name="offset">The offset within the memory map to read from.</param>
  /// <param name="bytes">The number of bytes to read.</param>
  /// <returns>The bytes read from the memory map.</returns>
  public byte[] ReadMemmap(byte offset, byte bytes) {
    long[] inArray = [offset, bytes];
    long[] outArray = _pawnIO.Execute("ioctl_ec_readmem", inArray, (int)Math.Ceiling(bytes / 8.0));
    byte[] retArray = new byte[bytes];
    Buffer.BlockCopy(outArray, 0, retArray, 0, bytes);
    return retArray;
  }

  /// <summary>
  /// Closes the underlying PawnIO module.
  /// </summary>
  public void Close() => _pawnIO.Close();
}
