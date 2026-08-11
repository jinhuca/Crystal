using Xunit;

namespace Crystal.Service.Process.Tests;

public class ProcessCategoryTests {
  [Theory]
  [InlineData(ProcessCategory.App, "Apps")]
  [InlineData(ProcessCategory.BackgroundProcess, "Background Processes")]
  [InlineData(ProcessCategory.WindowsProcess, "Windows Processes")]
  public void ToDisplayName_ReturnsGroupHeader(ProcessCategory category, string expected) {
    Assert.Equal(expected, category.ToDisplayName());
  }
}
