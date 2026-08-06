# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What Crystal is

Crystal is a Windows hardware-monitoring desktop app (in the vein of HWiNFO): it reads live
sensors — temperatures, clocks, voltages, power, load, fan speeds — from CPU, GPU, memory,
storage, network and other components, and presents them in a WPF dashboard of per-component
tiles that expand into detail windows.

## Build, run, test

The repo targets **.NET 10** and builds with the `.slnx` solution format. Some projects (P/Invoke
native, Telemetry) are architecture-specific; managed projects default to **x64** when no platform
is given (see `Directory.Build.props`) because CsWin32 can't generate P/Invokes for AnyCPU.

```bash
# Build the whole solution
dotnet build Crystal.slnx

# Run the app (the current WPF shell)
dotnet run --project Shell/Crystal.Shell/Crystal.Shell.csproj

# Run all tests
dotnet test Crystal.slnx

# Run one test project
dotnet test UnitTests/Crystal.Service.Sensors.Tests/Crystal.Service.Sensors.Tests.csproj

# Run a single test by name
dotnet test UnitTests/Crystal.Service.Sensors.Tests/Crystal.Service.Sensors.Tests.csproj --filter "FullyQualifiedName~SensorMonitor"
```

- All build output goes to `Build/` (redirected in `Directory.Build.props`), not per-project `bin/`.
- `dotnet` cannot restore the C++ `Crystal.Provider.PInvokes.vcxproj`; the expected `NU1503`
  restore-skip warning is downgraded in `Directory.Build.rsp`. Build that project via MSBuild /
  Visual Studio if you need the native DLL.
- Tests use **xUnit**; reactive code is tested with `Microsoft.Reactive.Testing` (`TestScheduler`).
- Focused sub-solutions exist for isolated work: `Crystal.Mmi.slnx`, `Crystal.Controls.slnx`.

## Running the app requires elevation

`Crystal.Shell` ships an `app.manifest` requesting **Administrator** rights. Ring-0 sensors
(CPU MSR voltage/power/temp, kernel ETW for processes) need it. Without elevation and the
**PawnIO** kernel driver installed, those sensors silently report no value; OS-performance-counter
sensors (e.g. total CPU load) still work. The app is **single-instance**: a second launch signals
the running instance to surface itself, then exits (it would otherwise contend for hardware handles).

## Architecture

The codebase is a layered pipeline. Data flows **Provider → Service → Module → Shell**:

- **`Infrastructures/`** — dependency-free primitives: constants (`RegionNames`, `ViewNames`,
  navigation names), converters, data structures.
- **`Providers/`** — raw hardware access, one project per source technology, not per component:
  - `Crystal.Provider.Telemetry` — the big one; a fork of **LibreHardwareMonitor**. Open a
    `Computer`, enable categories, `Update()`, read `ISensor`s. Uses PawnIO for MSR access.
  - `Crystal.Provider.CpuId`, `.Smbios`, `.Mmi` (WMI/MI), `.Etw`, `.PInvokes` (C++ native DLL
    with CsWin32; see `NativeMethods.txt` for generated P/Invokes).
- **`Services/`** — component-oriented layer that composes providers into a polling model and
  exposes it as a reactive stream. Key pattern: a **`*Monitor`** wraps a source and publishes an
  `IObservable<Snapshot>` via `Observable.Interval(...).Select(...).Publish().RefCount()` — the
  poll timer runs only while subscribed. Monitors are **singletons** (they own polling lifetime and
  hardware sessions) and default to a **1-second** cadence. See `Crystal.Service.Sensors`
  (system-wide `SensorMonitor`) and `Crystal.Service.Cpu`.
- **`Modules/`** — one Prism `IModule` per component (Cpu, Gpu, Memory, Storage, Bios, Network,
  Process, Resource). Each module's `RegisterTypes` wires the provider→service→model→view-model
  chain, and `OnInitialized` injects a compact **summary tile** into its dashboard region while
  registering a full **detail view** for navigation.
- **`UI/`** — shared WPF controls (`Crystal.Controls`) and value converters (`Crystal.WpfConverters`).
- **`Shell/`** — `Crystal.Shell` is the Prism (Unity) application: the app entry point.

### Prism / WPF conventions to know

- The app is a **Prism `PrismApplication`** using the **Unity** container. `App.xaml.cs`
  registers shell-level singletons, populates the module catalog, then in `OnInitialized` warms
  the heavy module singletons on a **background thread behind a loading overlay** (constructing them
  opens ring-0 sessions and would freeze the UI thread), before swapping in the dashboard.
- **Optional constructor parameters are not injected by Unity.** Types with optional ctor params
  (a nullable telemetry source, a `TimeSpan?` poll interval, an `IScheduler?`) must be registered
  via a **factory lambda** (`containerRegistry.Register<T>(cp => new T(...))`), or sensors silently
  read empty / the wrong cadence is used. This pattern recurs across modules and services.
- View↔ViewModel wiring is explicit: views set `AutoWireViewModel="True"`, but VMs live under
  `.ViewModels.Implementations` and resolve by interface, so each module maps its views with
  `ViewModelLocationProvider.Register<TView>(...)`. Views are registered per-instance (not
  singleton) so their live sample buffers aren't shared.
- Navigation goes through the singleton `NavigationController` / `DetailWindowService`; detail
  window placement (position/size/pin) persists across sessions via `WindowLayoutStore` and is
  restored on startup.

## UI style

The monitoring UI should read as **HWiNFO-style: technical and dense**, not modern/rounded. Favor
compact, information-rich layouts over spacious card design.
