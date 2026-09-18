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
  private readonly int _groups;
  private const int ThreadsPerGroup = 256;
  private readonly int _iterations;
  private readonly int _dispatches;

  public GpuComputeBenchmark(int groups = 4096, int iterations = 2048, int dispatches = 8) {
    _groups = groups;
    _iterations = iterations;
    _dispatches = dispatches;
  }

  public string Id => "gpu.compute";
  public string Name => "Compute (DirectCompute FMA)";
  public BenchmarkCategory Category => BenchmarkCategory.Gpu;
  public string Description => "Direct3D 11 compute-shader fused multiply-add throughput.";
  public string Unit => "GFLOPS";
  public bool HigherIsBetter => true;

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
        } catch (OperationCanceledException) {
          throw;
        } catch (Exception ex) {
          return BenchmarkResult.Failure(Id, ex.Message);
        } finally {
          uav?.Dispose();
          staging?.Dispose();
          output?.Dispose();
          shader?.Dispose();
          context?.Dispose();
          device?.Dispose();
        }
      }, ct);

  // Copies the UAV to the staging buffer and maps it for read — Map blocks the CPU until the GPU
  // has finished producing the data, which is how we force all queued dispatches to complete.
  private static void ForceCompletion(ID3D11DeviceContext context, ID3D11Buffer staging, ID3D11Buffer output) {
    context.CopyResource(staging, output);
    context.Flush();
    var mapped = context.Map(staging, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
    context.Unmap(staging, 0);
    GC.KeepAlive(mapped);
  }
}
