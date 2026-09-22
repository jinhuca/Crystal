using System.Diagnostics;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// GPU compute suite via Direct3D 11 DirectCompute. A compute shader runs a tight fused
/// multiply-add loop across ~1M threads; the batch is dispatched repeatedly and a staging read-back
/// (a blocking <c>Map</c>) forces GPU completion so the wall-clock covers real device work. The
/// score is sustained single-precision GFLOPS. If no hardware Direct3D 11 device can be created
/// (headless host, no GPU, denied), it returns a failed result rather than throwing.
/// </summary>
public sealed class GpuComputeBenchmark : IBenchmark {
  /// <summary>
  /// The number of thread groups to dispatch. Each group has 256 threads, so the total number of
  /// threads is _groups * ThreadsPerGroup.
  /// </summary>
  private readonly int _groups;

  /// <summary>
  /// The number of threads per group. This is a constant value of 256, which is a common choice for compute shaders.
  /// </summary>
  private const int ThreadsPerGroup = 256;

  /// <summary>
  /// The number of iterations of the fused multiply-add loop to perform in each thread. Each iteration performs 
  /// 8 floating-point operations (4 fused multiply-adds).
  /// </summary>
  private readonly int _iterations;

  /// <summary>
  /// The number of dispatches to perform. Each dispatch runs the compute shader across all threads, and the total 
  /// number of floating-point operations is calculated based on the number of threads, iterations, and dispatches.
  /// </summary>
  private readonly int _dispatches;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuComputeBenchmark"/> class with the specified number of groups, 
  /// iterations, and dispatches.
  /// </summary>
  /// <param name="groups">The number of thread groups to dispatch.</param>
  /// <param name="iterations">The number of iterations of the fused multiply-add loop to perform in each thread.</param>
  /// <param name="dispatches">The number of dispatches to perform.</param>
  public GpuComputeBenchmark(int groups = 4096, int iterations = 2048, int dispatches = 8) {
    _groups = groups;
    _iterations = iterations;
    _dispatches = dispatches;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite, which is "gpu.compute".
  /// </summary>
  public string Id => "gpu.compute";

  /// <summary>
  /// Gets the display name for this benchmark suite, which is "Compute (DirectCompute FMA)".
  /// </summary>
  public string Name => "Compute (DirectCompute FMA)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is GPU.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Gpu;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures, which is "Direct3D 11 
  /// compute-shader fused multiply-add throughput.".
  /// </summary>
  public string Description => "Direct3D 11 compute-shader fused multiply-add throughput.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is GFLOPS (giga 
  /// floating-point operations per second).
  /// </summary>
  public string Unit => "GFLOPS";

  /// <summary>
  /// Gets a value indicating whether a higher score is better for this benchmark suite. 
  /// For throughput benchmarks like this one, a higher score indicates better performance.
  /// </summary>
  public bool HigherIsBetter => true;

  /// <summary>
  /// Runs the GPU compute benchmark asynchronously, reporting progress and supporting cancellation. 
  /// It creates a Direct3D 11 device, compiles a compute shader, dispatches it multiple times, and 
  /// measures the sustained GFLOPS achieved. If no hardware device is available, it returns a failed result.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>The asynchronous task representing the benchmark result.</returns>
  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
    Task.Run(() => {
      progress.Report(BenchmarkProgress.At(0, "Creating Direct3D device"));

      ID3D11Device? device = null;
      ID3D11DeviceContext? context = null;
      ID3D11ComputeShader? shader = null;
      ID3D11Buffer? output = null;
      ID3D11Buffer? staging = null;
      ID3D11UnorderedAccessView? uav = null;

      try {
        var levels = new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };
        var result = D3D11.D3D11CreateDevice(
            null, DriverType.Hardware, DeviceCreationFlags.None, levels,
            out device, out context);
        if (result.Failure || device is null || context is null)
          return BenchmarkResult.Failure(Id, "No Direct3D 11 hardware device is available on this host.");

        // 8 FLOPs per loop iteration (4 fused multiply-adds). [loop] keeps it a real loop rather
        // than unrolling the constant bound into gigantic bytecode.
        string hlsl = $$"""
            RWStructuredBuffer<float> Output : register(u0);
            [numthreads({{ThreadsPerGroup}}, 1, 1)]
            void CSMain(uint3 tid : SV_DispatchThreadID) {
              float a = (float)tid.x * 1e-6f + 1.0f;
              float b = 0.9999994f;
              [loop]
              for (int i = 0; i < {{_iterations}}; i++) {
                a = a * b + 1.0f;
                a = a * b + 1.0f;
                a = a * b + 1.0f;
                a = a * b + 1.0f;
              }
              Output[tid.x] = a;
            }
            """;

        var compileResult = Compiler.Compile(hlsl, "CSMain", "gpu.compute.hlsl", "cs_5_0",
            out Blob? codeBlob, out Blob? errorBlob);
        if (compileResult.Failure || codeBlob is null) {
          string msg = errorBlob is not null ? errorBlob.AsString() : "Compute shader compilation failed.";
          return BenchmarkResult.Failure(Id, msg);
        }

        shader = device.CreateComputeShader(codeBlob.AsBytes());
        codeBlob.Dispose();
        errorBlob?.Dispose();

        uint threads = (uint)(_groups * ThreadsPerGroup);
        uint byteSize = threads * sizeof(float);

        output = device.CreateBuffer(new BufferDescription {
          ByteWidth = byteSize,
          BindFlags = BindFlags.UnorderedAccess,
          Usage = ResourceUsage.Default,
          MiscFlags = ResourceOptionFlags.BufferStructured,
          StructureByteStride = sizeof(float),
        });
        staging = device.CreateBuffer(new BufferDescription {
          ByteWidth = byteSize,
          BindFlags = BindFlags.None,
          Usage = ResourceUsage.Staging,
          CPUAccessFlags = CpuAccessFlags.Read,
        });
        uav = device.CreateUnorderedAccessView(output, new UnorderedAccessViewDescription {
          Format = Format.Unknown,
          ViewDimension = UnorderedAccessViewDimension.Buffer,
          Buffer = new BufferUnorderedAccessView { FirstElement = 0, NumElements = threads },
        });

        context.CSSetShader(shader);
        context.CSSetUnorderedAccessView(0, uav);

        // Warm-up dispatch + blocking read-back (untimed): compiles the pipeline, pages in.
        context.Dispatch((uint)_groups, 1, 1);
        ForceCompletion(context, staging, output);

        double flopsPerDispatch = (double)threads * _iterations * 8.0;
        var sw = Stopwatch.StartNew();
        for (int d = 0; d < _dispatches; d++) {
          ct.ThrowIfCancellationRequested();
          context.Dispatch((uint)_groups, 1, 1);
          progress.Report(BenchmarkProgress.At((d + 1) / (double)_dispatches, $"Dispatch {d + 1}/{_dispatches}"));
        }
        ForceCompletion(context, staging, output); // Blocks until every queued dispatch retires.
        sw.Stop();

        double gflops = flopsPerDispatch * _dispatches / sw.Elapsed.TotalSeconds / 1e9;
        string detail = $"{threads / 1e6:N1}M threads · {_dispatches} dispatches in {sw.Elapsed.TotalSeconds:N2} s";
        return BenchmarkResult.Success(Id, gflops, Unit, detail, sw.Elapsed);
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        return BenchmarkResult.Failure(Id, ex.Message);
      }
      finally {
        uav?.Dispose();
        staging?.Dispose();
        output?.Dispose();
        shader?.Dispose();
        context?.Dispose();
        device?.Dispose();
      }
    }, ct);

  /// <summary>
  /// Copies the UAV to the staging buffer and maps it for read — Map blocks the CPU until the GPU
  /// has finished producing the data, which is how we force all queued dispatches to complete.
  /// </summary>
  /// <param name="context">The device context.</param>
  /// <param name="staging">The staging buffer.</param>
  /// <param name="output">The output buffer.</param>
  private static void ForceCompletion(ID3D11DeviceContext context, ID3D11Buffer staging, ID3D11Buffer output) {
    context.CopyResource(staging, output);
    context.Flush();
    var mapped = context.Map(staging, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
    context.Unmap(staging, 0);
    GC.KeepAlive(mapped);
  }
}
