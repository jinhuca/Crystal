# Crystal.Provider.Telemetry

Cross-platform hardware telemetry for the Crystal project. Reads temperatures, fan speeds,
voltages, load, power and clock speeds from CPUs, GPUs, motherboards, memory, storage, PSUs
and network adapters. Based on a fork of
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor).

## Installation

```
dotnet add package Crystal.Provider.Telemetry
```

Target framework: `net10.0`. Supported runtimes: `win-x64`, `win-x86`, `win-arm64`,
`linux-x64`, `linux-arm64`.

## Usage

Open a `Computer`, enable the hardware categories you need, then update and read sensors:

```csharp
using Crystal.Provider.Telemetry.Hardware;

using var computer = new Computer {
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMotherboardEnabled = true,
    IsMemoryEnabled = true,
    IsStorageEnabled = true,
};

computer.Open();

foreach (IHardware hardware in computer.Hardware) {
    hardware.Update();
    foreach (ISensor sensor in hardware.Sensors) {
        Console.WriteLine($"{hardware.Name} / {sensor.Name} ({sensor.SensorType}): {sensor.Value}");
    }
}
```

Always dispose the `Computer` (shown above via `using`) so hardware handles and any loaded
drivers are released.

## Elevated / low-level sensors

Some readings come from CPU model-specific registers (MSRs) and other ring-0 sources — on
Windows these include CPU **voltage, package power, temperature and bus/clock speed**. They
are accessed through the [PawnIO](https://pawnio.eu) kernel driver, whose signed modules ship
embedded in this package.

For those sensors to return values:

1. The **PawnIO driver must be installed** on the machine.
2. The host process must run **with Administrator privileges**.

If either condition is not met, the driver handle cannot be opened, the MSR modules are never
loaded, and the affected sensors report no value. Sensors sourced from OS performance counters
(such as CPU total load) do not require elevation and work without the driver.

## License

Distributed under the [MPL-2.0](https://www.mozilla.org/en-US/MPL/2.0/) license, inherited
from the upstream LibreHardwareMonitor project.
