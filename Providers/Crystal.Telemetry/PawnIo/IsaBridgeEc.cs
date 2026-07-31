using System.Diagnostics;

namespace Crystal.Telemetry.PawnIo;
/// <summary>
/// Represents the memory-mapped I/O (MMIO) access state of the Super I/O chip exposed through the ISA bridge.
/// </summary>
public enum MMIOState {
  /// <summary>
  /// The MMIO state is unknown or could not be determined.
  /// </summary>
  Unknown = -999,

  /// <summary>
  /// The original MMIO state as configured before any modification.
  /// </summary>
  MMIO_Original = -1,

  /// <summary>
  /// MMIO access is disabled.
  /// </summary>
  MMIO_Disabled = 0,

  /// <summary>
  /// MMIO access is enabled at the 0x2E index port.
  /// </summary>
  MMIO_Enabled2E = 1,

  /// <summary>
  /// MMIO access is enabled at the 0x4E index port.
  /// </summary>
  MMIO_Enabled4E = 2,

  /// <summary>
  /// MMIO access is enabled at both the 0x2E and 0x4E index ports.
  /// </summary>
  MMIO_EnabledBoth = 3
};

/// <summary>
/// Describes a single Super I/O memory-mapped I/O region discovered through the ISA bridge.
/// </summary>
public struct MMIOMapping {
  /// <summary>
  /// The zero-based index of this Super I/O mapping.
  /// </summary>
  public int Index;

  /// <summary>
  /// The base physical address of the mapped MMIO region.
  /// </summary>
  public long BaseAddress;

  /// <summary>
  /// The size, in bytes, of the mapped Super I/O region.
  /// </summary>
  public long SuperIoSize;

  /// <summary>
  /// The identifier of the Super I/O chip associated with this mapping.
  /// </summary>
  public long ChipId;
}

/// <summary>
/// Provides access to a Super I/O embedded controller through the PawnIO ISA bridge driver module,
/// allowing discovery, mapping, and memory-mapped I/O read/write operations.
/// </summary>
public class IsaBridgeEc {
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(IsaBridgeEc).Assembly, $"{nameof(Crystal.Telemetry)}.Resources.PawnIo.IsaBridgeEC.bin");

  /// <summary>
  /// Queries the driver for the Super I/O MMIO regions and returns the first and second discovered mappings.
  /// </summary>
  /// <param name="firstMmio">When this method returns, contains the first discovered MMIO mapping.</param>
  /// <param name="secondMmio">When this method returns, contains the second discovered MMIO mapping.</param>
  /// <returns><see langword="true" /> if at least one mapping with a non-zero base address was found; otherwise, <see langword="false" />.</returns>
  // ioctl_find_superio_mmio
  public bool FindSuperIoMMIO(out MMIOMapping firstMmio, out MMIOMapping secondMmio) {
    long[] outArray = new long[6];
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_find_superio_mmio", [], 0, outArray, 6, out uint returnSize);

    Log($"FindSuperIoMMIO statusCode: {ntStatusCode}");

    if (ntStatusCode != 0) {
      firstMmio = default;
      secondMmio = default;
      return false;
    }

    firstMmio = new MMIOMapping {
      Index = 0,
      BaseAddress = outArray[0],
      SuperIoSize = outArray[1],
      ChipId = outArray[2]
    };

    secondMmio = new MMIOMapping {
      Index = 1,
      BaseAddress = outArray[3],
      SuperIoSize = outArray[4],
      ChipId = outArray[5]
    };

    Log($"First MMIO - BaseAddress: 0x{firstMmio.BaseAddress:X}, SuperIoSize: 0x{firstMmio.SuperIoSize:X}, ChipId: 0x{firstMmio.ChipId:X}");
    Log($"Second MMIO - BaseAddress: 0x{secondMmio.BaseAddress:X}, SuperIoSize: 0x{secondMmio.SuperIoSize:X}, ChipId: 0x{secondMmio.ChipId:X}");

    return firstMmio.BaseAddress != 0 || secondMmio.BaseAddress != 0;
  }

  /// <summary>
  /// Reads a byte from the specified Super I/O MMIO region at the given offset.
  /// </summary>
  /// <param name="superIoIndex">The index of the Super I/O mapping to read from.</param>
  /// <param name="offset">The offset within the MMIO region to read.</param>
  /// <param name="size">The size, in bytes, of the access to perform.</param>
  /// <param name="value">When this method returns, contains the byte read from the device.</param>
  /// <returns><see langword="true" /> if the read succeeded; otherwise, <see langword="false" />.</returns>
  //ioctl_access_superio_mmio
  public bool ReadMmio(long superIoIndex, long offset, long size, out byte value) {
    long[] inArray = new long[5] {
              superIoIndex, // superio index
              offset, // offset
              size, // size
              0, // is write?
              0 // value (ignored for read)
          };

    long[] outarray = new long[1];

    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_access_superio_mmio", inArray, 5, outarray, 1, out uint returnSize);

    Log($"ReadMmio statusCode: {ntStatusCode}, Read Value: 0x{outarray[0]:X} at SuperIoIndex {superIoIndex}, offset {offset}, size {size}");

    value = (byte)outarray[0];

    return ntStatusCode == 0;
  }

  /// <summary>
  /// Writes a byte to the specified Super I/O MMIO region at the given offset.
  /// </summary>
  /// <param name="superIoIndex">The index of the Super I/O mapping to write to.</param>
  /// <param name="offset">The offset within the MMIO region to write.</param>
  /// <param name="size">The size, in bytes, of the access to perform.</param>
  /// <param name="value">The byte value to write to the device.</param>
  /// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
  //ioctl_access_superio_mmio
  public bool WriteMmio(long superIoIndex, long offset, long size, byte value) {
    long[] inArray = new long[5] {
              superIoIndex, // superio index
              offset, // offset
              size, // size
              1, // is write?
              value // value
          };

    long[] outarray = new long[1];

    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_access_superio_mmio", inArray, 5, outarray, 1, out uint returnSize);

    Log($"WriteMmio statusCode: {ntStatusCode}, Written Value: 0x{value:X} at SuperIoIndex {superIoIndex}, offset {offset}, size {size}");

    return ntStatusCode == 0;
  }

  /// <summary>
  /// Maps the Super I/O MMIO regions so they can be accessed.
  /// </summary>
  /// <returns><see langword="true" /> if the mapping operation succeeded; otherwise, <see langword="false" />.</returns>
  // ioctl_map_superio_mmio
  public bool Map() {
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_map_superio_mmio", [], 0, [], 0, out uint returnSize);

    Log($"Map statusCode: {ntStatusCode}");

    return ntStatusCode == 0;
  }

  /// <summary>
  /// Unmaps the previously mapped Super I/O MMIO regions.
  /// </summary>
  /// <returns><see langword="true" /> if the unmapping operation succeeded; otherwise, <see langword="false" />.</returns>
  // ioctl_unmap_superio_mmio
  public bool Unmap() {
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_unmap_superio_mmio", [], 0, [], 0, out uint returnSize);

    Log($"Unmap statusCode: {ntStatusCode}");

    return ntStatusCode == 0;
  }

  /// <summary>
  /// Retrieves the original MMIO state of the Super I/O region as it was before any modification.
  /// </summary>
  /// <param name="state">When this method returns, contains the original MMIO state.</param>
  /// <returns><see langword="true" /> if the state was retrieved successfully; otherwise, <see langword="false" />.</returns>
  // ioctl_access_superio_mmio
  public bool GetOriginalState(out MMIOState state) {
    state = MMIOState.Unknown;
    long[] outArray = new long[1];
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_iomem_mmio_get_org_state", [], 0, outArray, 1, out uint returnSize);

    Log($"GetOriginalState statusCode: {ntStatusCode}");


    if (ntStatusCode != 0)
      return false;

    Log($"Original MMIO State: {(MMIOState)outArray[0]}");

    state = (MMIOState)outArray[0];
    return true;
  }

  /// <summary>
  /// Attempts to retrieve the current MMIO state of the Super I/O region.
  /// </summary>
  /// <param name="state">When this method returns, contains the current MMIO state.</param>
  /// <returns><see langword="true" /> if the state was retrieved successfully; otherwise, <see langword="false" />.</returns>
  public bool TryGetCurrentState(out MMIOState state) {
    state = MMIOState.Unknown;
    long[] outArray = new long[1];
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_iomem_mmio_get_cur_state", [], 0, outArray, 1, out uint returnSize);

    if (ntStatusCode != 0)
      return false;

    state = (MMIOState)outArray[0];
    return true;
  }

  /// <summary>
  /// Attempts to set the MMIO state of the Super I/O region.
  /// </summary>
  /// <param name="state">The MMIO state to apply.</param>
  /// <returns><see langword="true" /> if the state was set successfully; otherwise, <see langword="false" />.</returns>
  public bool TrySetState(MMIOState state) {
    long[] inArray = new long[1];
    inArray[0] = (long)state;
    int ntStatusCode = _pawnIO.ExecuteHr("ioctl_iomem_mmio_set_state", inArray, 1, [], 0, out uint returnSize);

    Log($"TrySetState to {state} statusCode: {ntStatusCode}");

    if (ntStatusCode != 0)
      return false;

    return true;
  }

  /// <summary>
  /// Closes the underlying PawnIO module and releases its resources.
  /// </summary>
  public void Close() => _pawnIO.Close();

  /// <summary>
  /// Writes a debug message to both the output window and a log file when ISA_BRIDGE_EC_DEBUG is defined.
  /// </summary>
  /// <remarks>The log entry is timestamped and appended to the file
  /// 'PawnIo_IsaBridgeEc_DebugLog.txt' in the application's working directory. This method only produces output
  /// when compiled with the ISA_BRIDGE_EC_DEBUG symbol defined.</remarks>
  /// <param name="message">The message to log. This should provide relevant information for debugging purposes.</param>
  [Conditional("DEBUG_LOG"), Conditional("ISA_BRIDGE_EC_DEBUG")]
  private static void Log(string message) {
    Debug.WriteLine(message);
  }
}
