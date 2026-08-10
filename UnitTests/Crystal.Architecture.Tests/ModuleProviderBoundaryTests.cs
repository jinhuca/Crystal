using System.Text;
using Xunit;

namespace Crystal.Architecture.Tests;

/// <summary>
/// Read-only boundary audit for the intended data flow <c>Provider -> Service -> Module</c>.
/// Prism modules should subscribe to Services, not reach into Providers. A module referencing a
/// Provider is only legitimate at its composition root (<c>RegisterTypes</c>, where a Prism module
/// IS the DI wiring point); anywhere else it is drift.
///
/// This test does not load module assemblies (they are WPF/x64 and heavy). It scans the module
/// source tree for <c>using Crystal.Provider.*</c> and classifies each hit into a tier:
///   Tier A  module root .cs (RegisterTypes)  -> allowed: composition-root wiring
///   Tier B  Models/*.cs                       -> drift: service work living inside the module
///   Tier C  ViewModels/*.cs or Views          -> violation: provider DTOs leaking into the VM
/// It prints the full catalog and asserts against a frozen baseline so the boundary can only
/// improve (a ratchet), never silently regress.
/// </summary>
public sealed class ModuleProviderBoundaryTests {
  private readonly ITestOutputHelper _out;
  public ModuleProviderBoundaryTests(ITestOutputHelper output) => _out = output;

  private enum Tier { A_CompositionRoot, B_ModuleModels, C_ViewModelLeak }

  private readonly record struct Hit(string Module, string File, int Line, string Namespace, Tier Tier);

  // Baseline captured 2026-08-10. Update DOWNWARD only as violations are removed.
  // 2026-08-10: SensorType leak removed (enums now mirrored in Infrastructure); C 3 -> 2. The two
  // remaining were Smbios DTOs BiosViewModel formatted, re-exported through FirmwareSnapshot.
  // 2026-08-10: those Smbios DTOs are now mirrored as neutral records/enums in Crystal.Service.Bios
  // (FirmwareInfoBuilder maps provider -> neutral at the boundary); BiosViewModel's 2 Smbios usings
  // removed, C 2 -> 0. No ViewModel references a provider anymore.
  // Tier C reached 0, so it is now asserted EXACTLY (not a <= ratchet): the presentation boundary is
  // closed and any new VM->Provider reference must instead bind a service-owned model.
  private const int BaselineTierC = 0;
  // 2026-08-10: GpuModule Models extracted into Crystal.Service.Gpu (builder/monitor/source moved);
  // GPU's 3 provider usings removed, B 16 -> 13.
  // 2026-08-10: MemoryModule Models extracted into Crystal.Service.Memory (builder/source/monitor
  // moved); Memory's 3 provider usings removed, B 13 -> 10.
  // 2026-08-10: NetworkModule Models extracted into Crystal.Service.Network (load/process sources,
  // snapshots moved; NetworkMonitor extracted); Network's 3 provider usings removed, B 10 -> 7.
  // 2026-08-10: StorageModule Models extracted into Crystal.Service.Storage (builder/load source,
  // snapshots moved; StorageMonitor extracted); Storage's 3 provider usings removed, B 7 -> 4.
  // 2026-08-10: ProcessModule polling sources extracted into Crystal.Service.Process (ProcessMonitor,
  // SystemStatsMonitor, samples/scanner moved); Process's provider usings removed, B 4 -> 0. No module
  // Models reference a provider anymore; the boundary is fully enforced.
  // Tier B reached 0, so it is now asserted EXACTLY (not a <= ratchet): the boundary is closed and any
  // new Models/->Provider reference must instead go into a Service. Tier C is still a downward ratchet.
  private const int BaselineTierB = 0;

  [Fact]
  public void ModuleProviderReferences_AreCatalogedAndDoNotRegress() {
    var hits = ScanModuleProviderUsings();
    var report = BuildReport(hits);
    _out.WriteLine(report);

    int tierC = hits.Count(h => h.Tier == Tier.C_ViewModelLeak);
    int tierB = hits.Count(h => h.Tier == Tier.B_ModuleModels);

    Assert.True(tierC == BaselineTierC,
        $"ViewModel->Provider leaks are {tierC}, expected exactly {BaselineTierC}. The presentation boundary is closed: VMs must bind service-owned models, not provider DTOs.\n{report}");
    Assert.True(tierB == BaselineTierB,
        $"Module Models->Provider references are {tierB}, expected exactly {BaselineTierB}. The boundary is closed: new polling/observable work belongs in a Service, not a module's Models/.\n{report}");
  }

  private static List<Hit> ScanModuleProviderUsings() {
    string modulesRoot = Path.Combine(RepoRoot(), "Modules");
    var hits = new List<Hit>();

    foreach (var file in Directory.EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories)) {
      if (IsGenerated(file)) continue;
      string moduleDir = ModuleNameFor(file, modulesRoot);
      var lines = File.ReadAllLines(file);
      for (int i = 0; i < lines.Length; i++) {
        string? ns = ProviderUsingNamespace(lines[i]);
        if (ns is null) continue;
        hits.Add(new Hit(moduleDir, RelPath(file), i + 1, ns, ClassifyTier(file)));
      }
    }
    return hits.OrderBy(h => h.Module).ThenBy(h => h.Tier).ThenBy(h => h.File).ToList();
  }

  private static string? ProviderUsingNamespace(string line) {
    string t = line.Trim();
    const string prefix = "using Crystal.Provider.";
    if (!t.StartsWith(prefix, StringComparison.Ordinal)) return null;
    return t.Substring("using ".Length).TrimEnd(';').Trim();
  }

  // A module's composition root is a .cs directly in the module dir (e.g. BiosModule.cs) — that is
  // where RegisterTypes lives. Anything under Models/ is service-like work; anything under
  // ViewModels/ or Views/ is a presentation-layer leak.
  private static Tier ClassifyTier(string file) {
    string dir = Path.GetFileName(Path.GetDirectoryName(file)!);
    if (dir.EndsWith("Module", StringComparison.Ordinal)) return Tier.A_CompositionRoot;
    if (dir is "ViewModels" or "Views") return Tier.C_ViewModelLeak;
    if (dir is "Models") return Tier.B_ModuleModels;
    // Nested dirs (e.g. Models/Foo) inherit by path substring.
    string path = file.Replace('\\', '/');
    if (path.Contains("/ViewModels/") || path.Contains("/Views/")) return Tier.C_ViewModelLeak;
    if (path.Contains("/Models/")) return Tier.B_ModuleModels;
    return Tier.A_CompositionRoot;
  }

  private static string BuildReport(List<Hit> hits) {
    var sb = new StringBuilder();
    sb.AppendLine("=== Module -> Provider reference catalog ===");
    foreach (var tier in new[] { Tier.C_ViewModelLeak, Tier.B_ModuleModels, Tier.A_CompositionRoot }) {
      var group = hits.Where(h => h.Tier == tier).ToList();
      sb.AppendLine($"\n[{tier}]  ({group.Count})");
      foreach (var h in group)
        sb.AppendLine($"  {h.Module,-14} {h.File}:{h.Line}  {h.Namespace}");
    }
    var noService = ModulesWithoutServiceReference();
    sb.AppendLine($"\nModules with NO Service project reference (own their data path): {string.Join(", ", noService)}");
    return sb.ToString();
  }

  private static IEnumerable<string> ModulesWithoutServiceReference() {
    string modulesRoot = Path.Combine(RepoRoot(), "Modules");
    foreach (var csproj in Directory.EnumerateFiles(modulesRoot, "*.csproj", SearchOption.AllDirectories)) {
      string text = File.ReadAllText(csproj);
      bool refsService = text.Contains("Services\\Crystal.Service") || text.Contains("Services/Crystal.Service");
      if (!refsService) yield return Path.GetFileNameWithoutExtension(csproj);
    }
  }

  private static string ModuleNameFor(string file, string modulesRoot) {
    string rel = Path.GetRelativePath(modulesRoot, file);
    int sep = rel.IndexOfAny(new[] { '/', '\\' });
    return sep < 0 ? rel : rel.Substring(0, sep);
  }

  private static bool IsGenerated(string file) {
    string name = Path.GetFileName(file);
    return name.EndsWith(".g.cs", StringComparison.Ordinal)
        || name.EndsWith(".g.i.cs", StringComparison.Ordinal)
        || name.EndsWith(".Designer.cs", StringComparison.Ordinal)
        || file.Replace('\\', '/').Contains("/obj/", StringComparison.OrdinalIgnoreCase);
  }

  private static string RelPath(string file) =>
      Path.GetRelativePath(RepoRoot(), file).Replace('\\', '/');

  private static string RepoRoot() {
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Crystal.slnx")))
      dir = dir.Parent;
    return dir?.FullName ?? throw new DirectoryNotFoundException("Could not locate Crystal.slnx above the test output directory.");
  }
}
