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
  /// <summary>
  /// Wildcard PDH counter path that matches every GPU Engine instance across all adapters.
  /// </summary>
  private const string CounterPath = @"\GPU Engine(*)\Utilization Percentage";

  /// <summary>
  /// Handle to the open PDH query, or <see cref="nint.Zero"/> if it could not be opened.
  /// </summary>
  private readonly nint _query;

  /// <summary>
  /// Handle to the wildcard GPU Engine counter added to <see cref="_query"/>.
  /// </summary>
  private readonly nint _counter;

  /// <summary>
  /// True once the query and counter opened and the baseline collection ran (see <see cref="IsAvailable"/>).
  /// </summary>
  private readonly bool _ready;

  // Reused across polls so a steady 1 Hz cadence doesn't churn the LOH with a fresh array buffer
  // each second; grown on demand when PDH reports it needs more room.
  private byte[] _buffer = new byte[16 * 1024];

  /// <summary>
  /// Set by <see cref="Dispose"/>; makes further <see cref="Sample"/> calls return the empty map.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Opens the PDH query and the wildcard GPU Engine counter, then primes a baseline collection so the
  /// first <see cref="Sample"/> one interval later has a prior reading to compute the utilization rate
  /// against. If either PDH call fails the sampler stays inert (<see cref="IsAvailable"/> is false) and
  /// the caller falls back to its other GPU source. Uses the English counter name so the path resolves
  /// on non-English machines.
  /// </summary>
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

  /// <summary>
  /// Pins <see cref="_buffer"/> and asks PDH to format the whole counter array into it. Split out so the
  /// caller can retry after growing the buffer when PDH returns <see cref="PdhMoreData"/>.
  /// </summary>
  /// <param name="bufferSize">In: the buffer's capacity in bytes. Out: the size PDH actually needs/used.</param>
  /// <param name="itemCount">Out: the number of counter items PDH wrote.</param>
  /// <returns>The PDH status code (0 on success, <see cref="PdhMoreData"/> when the buffer is too small).</returns>
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

  /// <summary>
  /// Reads the underscore-delimited token that follows <paramref name="key"/> in the instance name —
  /// e.g. with key <c>pid_</c> in <c>pid_1234_luid_...</c> it yields <c>1234</c>.
  /// </summary>
  /// <param name="instance">The full PDH instance name to scan.</param>
  /// <param name="key">The token prefix to search for (including its trailing underscore).</param>
  /// <param name="token">Out: the extracted token, or empty when the key is absent.</param>
  /// <returns>True if a non-empty token was found after the key.</returns>
  private static bool TryReadToken(string instance, string key, out string token) {
    token = string.Empty;
    int start = instance.IndexOf(key, StringComparison.Ordinal);
    if (start < 0) return false;

    start += key.Length;
    int end = instance.IndexOf('_', start);
    token = end < 0 ? instance[start..] : instance[start..end];
    return token.Length > 0;
  }

  /// <summary>
  /// Closes the underlying PDH query. Idempotent; safe to call more than once.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    if (_query != nint.Zero) PdhCloseQuery(_query);
  }

  /// <summary>
  /// Shared empty result returned whenever there is no usable per-PID data to hand back.
  /// </summary>
  private static readonly IReadOnlyDictionary<uint, double> EmptyMap =
      new Dictionary<uint, double>();

  /// <summary>
  /// PDH_FMT_DOUBLE: format flag asking PDH to return counter values as doubles.
  /// </summary>
  private const uint PdhFmtDouble = 0x00000200;

  /// <summary>
  /// PDH_MORE_DATA: status meaning the supplied buffer was too small and must be grown.
  /// </summary>
  private const uint PdhMoreData = 0x800007D2;

  /// <summary>
  /// Managed mirror of PDH_FMT_COUNTERVALUE for the double format: a status word plus the value.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  private struct PdhFmtCounterValueDouble {
    public uint CStatus;
    // 4 bytes of padding here on x64 so the double lands on its natural 8-byte boundary; the
    // sequential layout inserts it automatically, matching PDH_FMT_COUNTERVALUE.
    public double doubleValue;
  }

  /// <summary>
  /// Managed mirror of PDH_FMT_COUNTERVALUE_ITEM: one array entry pairing an instance name
  /// pointer with its formatted value.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  private struct PdhFmtCounterValueItem {
    public nint szName;
    public PdhFmtCounterValueDouble FmtValue;
  }

  /// <summary>
  /// Opens a new PDH query handle. Returns 0 on success.
  /// </summary>
  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhOpenQuery(string? dataSource, nint userData, out nint query);

  /// <summary>
  /// Adds a counter to the query using its English (locale-independent) name. Returns 0 on success.
  /// </summary>
  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhAddEnglishCounter(nint query, string counterPath, nint userData, out nint counter);

  /// <summary>
  /// Collects a fresh sample for every counter in the query; the rate is computed from
  /// successive collections. Returns 0 on success.
  /// </summary>
  [DllImport("pdh.dll")]
  private static extern uint PdhCollectQueryData(nint query);

  /// <summary>
  /// Formats every instance of a wildcard counter into the caller's buffer. Returns
  /// <see cref="PdhMoreData"/> when the buffer is too small.
  /// </summary>
  [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
  private static extern uint PdhGetFormattedCounterArray(
      nint counter, uint format, ref uint bufferSize, out uint itemCount, nint itemBuffer);

  /// <summary>
  /// Closes a PDH query and frees its counters. Returns 0 on success.
  /// </summary>
  [DllImport("pdh.dll")]
  private static extern uint PdhCloseQuery(nint query);
}
