using Crystal.Service.Benchmark;
using Xunit;

namespace Crystal.Service.Benchmark.Tests;

public sealed class BenchmarkCatalogTests {
  [Fact]
  public void All_CoversEveryCategory() {
    var catalog = new BenchmarkCatalog();

    Assert.NotEmpty(catalog.All);
    foreach (var category in Enum.GetValues<BenchmarkCategory>())
      Assert.Contains(catalog.All, b => b.Category == category);
  }

  [Fact]
  public void All_SuiteIdsAreUnique() {
    var catalog = new BenchmarkCatalog();

    var ids = catalog.All.Select(b => b.Id).ToList();
    Assert.Equal(ids.Count, ids.Distinct().Count());
  }
}
