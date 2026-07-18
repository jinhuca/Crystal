# Crystal.Plot2D.Tests

xUnit test project for the Crystal.Plot2D library.

## Project Structure

```
Unit Tests/
└── Crystal.Plot2D.Tests/
    ├── Crystal.Plot2D.Tests.csproj
    ├── PlotInitializationTests.cs     # Tests for Plot creation and basic properties
    ├── SeriesTests.cs                 # Tests for data series operations
    ├── AxisTests.cs                   # Tests for axis configuration
    └── README.md                       # This file
```

## Setup Instructions

### 1. Create the folder structure

```
Your Solution Root/
├── Unit Tests/
│   └── Crystal.Plot2D.Tests/
```

### 2. Copy files

Copy the following files into the `Crystal.Plot2D.Tests` folder:

- `Crystal.Plot2D.Tests.csproj`
- `PlotInitializationTests.cs`
- `SeriesTests.cs`
- `AxisTests.cs`

### 3. Add project to solution

In Visual Studio:
1. Right-click on your solution
2. Select "Add" → "Existing Project"
3. Navigate to `Crystal.Plot2D.Tests.csproj`
4. Click Open

Or use the .NET CLI:
```bash
dotnet sln add ./Unit\ Tests/Crystal.Plot2D.Tests/Crystal.Plot2D.Tests.csproj
```

### 4. Verify the project reference

The `.csproj` includes a reference to Crystal.Plot2D:
```xml
<ProjectReference Include="..\..\Shared\Crystal.Plot2D\Crystal.Plot2D.csproj" />
```

Adjust the path if your folder structure differs.

## Running Tests

### Visual Studio
- **Test Explorer**: View → Test Explorer (Ctrl+E, T)
- **Run All**: Run All Tests in Test Explorer
- **Run Single**: Right-click a test → Run

### Command Line
```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter FullyQualifiedName~PlotInitializationTests

# Run and generate coverage report
dotnet test /p:CollectCoverage=true
```

## Test Files Overview

### PlotterInitializationTests.cs
- Plotter object creation
- Default component properties
- Legend, AxisGrid, and Navigation components
- Axis existence

### AxisTests.cs
- Main horizontal and vertical axes
- Axis visibility settings (Visible, Hidden, Collapsed)
- Changing axes dynamically
- Navigation and context menu components

### SeriesTests.cs (PlotterLegendTests & PlotterNavigationTests)
- Legend visibility control
- AxisGrid properties
- Mouse navigation
- Keyboard navigation
- Axis navigation

## Adding More Tests

To add tests for additional Crystal.Plot2D features:

1. Create a new `*Tests.cs` file in the project
2. Add the namespace: `namespace Crystal.Plot2D.Tests;`
3. Create a public test class: `public class MyFeatureTests`
4. Add test methods with `[Fact]` or `[Theory]` attributes
5. Run tests in Visual Studio Test Explorer

### Example:
```csharp
[Fact]
public void MyTest_DescribesWhatItTests()
{
    // Arrange
    var plotter = new Plotter();

    // Act
    plotter.MainHorizontalAxisVisibility = Visibility.Hidden;

    // Assert
    Assert.Equal(Visibility.Hidden, plotter.MainHorizontalAxisVisibility);
}
```

## Dependencies

- **xUnit 2.7.0**: Testing framework
- **Microsoft.NET.Test.Sdk 17.9.0**: Test SDK
- **Crystal.Plot2D**: Project being tested (referenced)

## Notes

- Tests use .NET 10 Windows (`net10.0-windows7.0`) to match Crystal.Plot2D
- Nullable reference types are enabled
- All tests follow the AAA (Arrange-Act-Assert) pattern
- Tests use Theory attributes for parameterized tests where applicable
- Windows 7.0+ required due to platform-specific dependencies
