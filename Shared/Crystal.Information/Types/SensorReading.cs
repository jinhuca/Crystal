using Crystal.Telemetry.Hardware;

namespace Crystal.Information.Types;

public record SensorReading(
    string HardwareName,
    HardwareType HardwareType,
    string SensorName,
    SensorType SensorType,
    float? Value,
    float? Min,
    float? Max,
    string? Unit);

