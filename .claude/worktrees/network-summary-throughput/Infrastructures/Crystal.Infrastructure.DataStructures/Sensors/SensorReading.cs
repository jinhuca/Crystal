using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Infrastructure.DataStructures.Sensors;

public record SensorReading(
    string HardwareName,
    HardwareType HardwareType,
    string SensorName,
    SensorType SensorType,
    float? Value,
    float? Min,
    float? Max,
    string? Unit);
