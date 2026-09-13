using System.Globalization;
using System.Runtime.InteropServices;

namespace Crystal.Service.Process;

/// <summary>
/// Per-process GPU utilization sampled from the Windows <c>GPU Engine</c> performance counters — the
/// exact source Task Manager reports on its Processes tab. Each counter instance is named
/// <c>pid_1234_luid_..._phys_0_eng_0_engtype_3d</c>; the <c>Utilization Percentage</c> value is a
/// running-time rate that PDH computes from the delta between two collections, so the first
/// <see cref="Sample"/> after construction returns nothing and steady values arrive from the second on.
/// <para>
/// This replaces the ETW DMA-packet timing for the GPU column: DMA packets only capture graphics
/// submissions and badly under-report compute/video/HAGS workloads (an AI or media process that
/// Task Manager shows at 59% read ~0% via ETW). The engine counters attribute real occupancy per PID.
/// </para>
/// <para>
/// Aggregation matches Task Manager: within one adapter (<c>phys</c>) the utilization of each engine
/// node of the same <c>engtype</c> is summed, then the maximum across engine types (and adapters) is
/// taken as the process's headline GPU%. Clamped to 0-100. Reading the counters needs no elevation.
/// </para>
/// <para>
/// Not thread-safe: <see cref="Sample"/> mutates a shared reusable buffer and the PDH query, and is
/// only ever called from <see cref="ProcessMonitor"/>'s single in-flight poll. If the PDH query can't
/// be opened the sampler stays inert and every <see cref="Sample"/> returns an empty map, letting the
/// caller fall back to the ETW value.
/// </para>
/// </summary>
public sealed class GpuProcessUsageSampler : IDisposable {
  private const string CounterPath = @"\GPU Engine(*)\Utilization Percentage";

  private readonly nint _query;
  private readonly nint _counter;
  private readonly bool _ready;

  // Reused across polls so a steady 1 Hz cadence doesn't churn the LOH with a fresh array buffer
  // each second; grown on demand when PDH reports it needs more room.
  private byte[] _buffer = new byte[16 * 1024];
  private bool _disposed;

  public GpuProcessUsageSampler() {
    // English counter names so the path resolves regardless of the machine's display language.
    if (PdhOpenQuery(null, nint.Zero, out _query) != 0) {
      return;
    }

    if (PdhAddEnglishCounter(_query, CounterPath, nint.Zero, out _counter) != 0) {
      PdhCloseQuery(_query);
      _query = nint.Zero;
      return;
    }

    // Prime the baseline: the utilization rate needs a prior collection to diff against, so the
    // first real Sample() (one interval later) already has a window to compute over.
    PdhCollectQueryData(_query);
    _ready = true;
  }

  /// <summary>
  /// Whether the GPU Engine counters were opened successfully. When false, <see cref="Sample"/>
  /// always returns an empty map and the caller should fall back to its other GPU source.
  /// </summary>
  public bool IsAvailable => _ready;

  /// <summary>
  /// Collects the counters and returns the current GPU utilization percentage per PID. Empty when the
  /// query is unavailable, the collection failed, or PDH has no valid data yet (the first sample).
  /// </summary>
  public IReadOnlyDictionary<uint, double> Sample() {
    if (!_ready || _disposed) return EmptyMap;

    if (PdhCollectQueryData(_query) != 0) return EmptyMap;

    uint bufferSize = (uint)_buffer.Length;
    uint itemCount = 0;

    var status = ReadArray(ref bufferSize, ref itemCount);
    if (status == PdhMoreData) {
      // PDH tells us exactly how many bytes it needs; grow and read once more.
      _buffer = new byte[bufferSize];
      status = ReadArray(ref bufferSize, ref itemCount);
    }

    if (status != 0 || itemCount == 0) return EmptyMap;

    // Per PID: sum utilization within each (adapter, engine-type) bucket, then keep the max bucket.
    var perPidPerBucket = new Dictionary<uint, Dictionary<string, double>>();

    var handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
    try {
      nint basePtr = handle.AddrOfPinnedObject();
      int itemSize = Marshal.SizeOf<PdhFmtCounterValueItem>();
      for (int i = 0; i < itemCount; i++) {
        var item = Marshal.PtrToStructure<PdhFmtCounterValueItem>(basePtr + i * itemSize);
        if (item.FmtValue.CStatus != 0 || item.szName == nint.Zero) continue;

        string? instance = Marshal.PtrToStringUni(item.szName);
        if (instance is null) continue;

        if (!TryParseInstance(instance, out uint pid, out string bucket)) continue;

        double value = item.FmtValue.doubleValue;
        if (value <= 0) continue;

        if (!perPidPerBucket.TryGetValue(pid, out var buckets)) {
          buckets = [];
          perPidPerBucket[pid] = buckets;
        }
        buckets[bucket] = buckets.TryGetValue(bucket, out var sum) ? sum + value : value;
      }
    } finally {
      handle.Free();
    }

    if (perPidPerBucket.Count == 0) return EmptyMap;

    var result = new Dictionary<uint, double>(perPidPerBucket.Count);
    foreach (var (pid, buckets) in perPidPerBucket) {
      double max = 0;
      foreach (var v in buckets.Values) {
        if (v > max) max = v;
      }
      result[pid] = max > 100 ? 100 : max;
    }
    return result;
  }

  private uint ReadArray(ref uint bufferSize, ref uint itemCount) {
    var handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
    try {
      return PdhGetFormattedCounterArray(
          _counter, PdhFmtDouble, ref bufferSize, out itemCount, handle.AddrOfPinnedObject());
    } finally {
      handle.Free();
    }
  }

  /// <summary>
  /// Extracts the PID and an "adapter + engine type" bucket key from a GPU Engine instance name such
  /// as <c>pid_11836_luid_0x00000000_0x0000C4E1_phys_0_eng_2_engtype_VideoDecode</c>. The bucket
  /// keys on <c>phys</c> (which adapter) and <c>engtype</c> (3D, Compute, VideoDecode, …) but not
  /// <c>eng</c> (the node index), so sibling engine nodes of the same type sum together.
  /// </summary>
  private static bool TryParseInstance(string instance, out uint pid, out string bucket) {
    pid = 0;
    bucket = string.Empty;

    if (!TryReadToken(instance, "pid_", out string pidToken) ||
        !uint.TryParse(pidToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out pid)) {
      return false;
    }

    // engtype runs to the end of the instance name; phys is a single token in the middle.
    int engTypeStart = instance.IndexOf("engtype_", StringComparison.Ordinal);
    string engType = engTypeStart >= 0 ? instance[(engTypeStart + "engtype_".Length)..] : "unknown";
    string phys = TryReadToken(instance, "phys_", out string physToken) ? physToken : "0";

    bucket = phys + "|" + engType;
    return true;
  }

  // Reads the underscore-delimited token that follows <paramref name="key"/> in the instance name.
  private static bool TryReadToken(string instance, string key, out string token) {
    token = string.Empty;
    int start = instance.IndexOf(key, StringComparison.Ordinal);
    if (start < 0) return false;

    start += key.Length;
    int end = instance.IndexOf('_', start);
    token = end < 0 ? instance[start..] : instance[start..end];
    return token.Length > 0;
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    if (_query != nint.Zero) PdhCloseQuery(_query);
  }

  private static readonly IReadOnlyDictionary<uint, double> EmptyMap =
      new Dictionary<uint, double>();

  private const uint PdhFmtDouble = 0x00000200;
  private const uint PdhMoreData = 0x800007D2;

  [StructLayout(LayoutKind.Sequential)]
  private struct PdhFmtCounterValueDouble {
    public uint CStatus;
    // 4 bytes of padding here on x64 so the double lands on its natural 8-byte boundary; the
    // sequential layout inserts it automatically, matching PDH_FMT_COUNTERVALUE.
    public double doubleValue;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  private struct PdhFmtCounterValueItem {
    public nint szName;
    public PdhFmtCounterValueDouble FmtValue;
  }

  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhOpenQuery(string? dataSource, nint userData, out nint query);

  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhAddEnglishCounter(nint query, string counterPath, nint userData, out nint counter);

  [DllImport("pdh.dll")]
  private static extern uint PdhCollectQueryData(nint query);

  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhGetFormattedCounterArray(
      nint counter, uint format, ref uint bufferSize, out uint itemCount, nint itemBuffer);

  [DllImport("pdh.dll")]
  private static extern uint PdhCloseQuery(nint query);
}
