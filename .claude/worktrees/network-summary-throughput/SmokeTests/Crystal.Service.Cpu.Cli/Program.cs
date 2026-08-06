using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Service.Cpu;

// Smoke test: wires the real providers into CpuInfoBuilder and prints the
// merged CPU inventory for this machine, then streams live CPU/core sensors
// read through the service's ICpuInfo.Sensors/Cores (backed by the Telemetry
// provider). Run on x64 Windows; MSR-backed sensors need Administrator.

using var telemetry = new TelemetryCpuSensorSource();
var builder = new CpuInfoBuilder(
    cpuId: new CpuIdProvider(),
    smbios: new SmbiosProcessorProvider(),
    wmi: new WmiHardwareProvider(),
    resolver: new CpuSpecsResolver(),
    telemetry: telemetry);

int iterations = 5;
if (args.Length > 0 && int.TryParse(args[0], out var n)) iterations = n;

ISystemCpuInfo system;
try {
  system = await builder.BuildAsync(CancellationToken.None);
}
catch (Exception ex) {
  Console.WriteLine($"Failed to build CPU info: {ex.Message}");
  Console.WriteLine(ex.StackTrace);
  return 1;
}

Console.WriteLine($"Detected {system.Sockets.Count} populated socket(s).\n");

foreach (var socket in system.Sockets) {
  PrintSocket(socket);
}

Console.WriteLine($"=== Live CPU Sensors ({iterations} samples @ 1s) ===");
Console.WriteLine("(temperature/clock/voltage/power read MSRs via the ring-0 driver — run elevated if they show n/a)\n");

// Each BuildAsync re-samples the telemetry source, so the merged ICpuInfo
// carries fresh Sensors/Cores every iteration.
for (int i = 0; i < iterations; i++) {
  var snapshot = i == 0 ? system : await builder.BuildAsync(CancellationToken.None);
  foreach (var socket in snapshot.Sockets) {
    PrintLiveSocket(socket, i + 1, iterations);
  }
  if (i < iterations - 1) await Task.Delay(1000);
}

return 0;

static void PrintSocket(ICpuInfo socket) {
  var s = socket.Specs;
  Console.WriteLine($"=== Socket #{socket.SocketIndex} ({socket.SocketDesignation ?? "unknown"}) ===");
  Console.WriteLine($"  Brand:            {s.BrandName ?? "(unknown)"}");
  Console.WriteLine($"  Vendor:           {s.VendorName ?? "(unknown)"}");
  Console.WriteLine($"  Family/Model/Step:{s.FamilyId}/{s.ModelId}/{s.SteppingId}");
  Console.WriteLine($"  Base Speed:       {Fmt(s.BaseSpeed, "MHz")}");
  Console.WriteLine($"  Bus Speed:        {Fmt(s.BusSpeed, "MHz")}");
  Console.WriteLine($"  Physical Cores:   {s.PhysicalCoreNum?.ToString() ?? "(unknown)"}");
  Console.WriteLine($"  Logical Cores:    {s.LogicalCoreNum?.ToString() ?? "(unknown)"}");
  Console.WriteLine($"  Virtualization:   supported={FmtBool(s.VirtualizationSupported)}  enabled={FmtBool(s.VirtualizationEnabled)}");

  PrintCache(s.CacheInfo);
  PrintInstructionSet(s.InstructionSet);
  Console.WriteLine();
}

static void PrintCache(CpuCacheInfo? cache) {
  if (cache is not { } c) {
    Console.WriteLine("  Cache:            (not reported)");
    return;
  }
  Console.WriteLine("  Cache:");
  Console.WriteLine($"    L1: {c.L1_cache_size} KB (line {c.L1_cache_line_size} B)");
  Console.WriteLine($"    L2: {c.L2_cache_size} KB (line {c.L2_cache_line_size} B)");
  Console.WriteLine($"    L3: {c.L3_cache_size} KB (line {c.L3_cache_line_size} B)");
}

static void PrintInstructionSet(CpuInstructionInfo? isa) {
  if (isa is not { } i) {
    Console.WriteLine("  Instruction Set:  (not reported)");
    return;
  }
  var supported = new List<string>();
  if (i.MMX) supported.Add("MMX");
  if (i.SSE) supported.Add("SSE");
  if (i.SSE2) supported.Add("SSE2");
  if (i.SSE3) supported.Add("SSE3");
  if (i.SSSE3) supported.Add("SSSE3");
  if (i.SSE41) supported.Add("SSE4.1");
  if (i.SSE42) supported.Add("SSE4.2");
  if (i.AVX) supported.Add("AVX");
  if (i.AVX2) supported.Add("AVX2");
  if (i.AVX512F) supported.Add("AVX-512F");
  if (i.FMA) supported.Add("FMA");
  if (i.AES) supported.Add("AES");
  if (i.SHA) supported.Add("SHA");
  if (i.BMI1) supported.Add("BMI1");
  if (i.BMI2) supported.Add("BMI2");

  Console.WriteLine($"  Instruction Set:  {(supported.Count > 0 ? string.Join(", ", supported) : "(none detected)")}");
}

static void PrintLiveSocket(ICpuInfo socket, int sample, int total) {
  Console.WriteLine($"[{sample}/{total}] Socket #{socket.SocketIndex} ({socket.Specs.BrandName ?? socket.SocketDesignation})");

  var cpu = socket.Sensors;
  PrintReading("Package Temp", cpu.PackageTemperature);
  PrintReading("Core Max Temp", cpu.CoreMaxTemperature);
  PrintReading("Total Load", cpu.TotalLoad);
  PrintReading("Bus Speed", cpu.CpuSpeed);
  PrintReading("Package Power", cpu.PackagePower);
  PrintReading("Core Voltage", cpu.Voltage);

  if (socket.Cores.Count > 0) {
    Console.WriteLine("  Per-core:");
    foreach (var core in socket.Cores) {
      PrintCore(core);
    }
  }

  Console.WriteLine();
}

static void PrintCore(ICoreInfo core) {
  var sp = core.Specs;
  var se = core.Sensors;
  string type = sp.Type is { } t ? t.ToString() : "?";
  string load = se.Load.Value is { } l ? $"{l,5:F1} %" : "  n/a";
  string clock = se.Speed.Value is { } c ? $"{c,7:F0} MHz" : "    n/a";
  string temp = se.Temperature.Value is { } tp ? $"{tp,5:F1} °C" : "  n/a";
  Console.WriteLine($"    Core {sp.CoreIndex,2} [{type,-11}] x{sp.ThreadCount}: load {load}   clock {clock}   temp {temp}");
}

static void PrintReading(string label, SensorReading reading) {
  if (reading.Value is not { } v) return;
  Console.WriteLine($"  {label,-14}: {v,8:F2} {reading.Unit}");
}

static string Fmt(float? value, string unit) => value is > 0 ? $"{value} {unit}" : "(unknown)";
static string FmtBool(bool? value) => value?.ToString() ?? "unknown";
