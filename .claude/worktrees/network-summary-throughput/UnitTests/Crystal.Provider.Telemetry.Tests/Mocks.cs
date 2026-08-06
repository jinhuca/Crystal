using System;
using System.Collections.Generic;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Provider.Telemetry.Tests;

/// <summary>Dictionary-backed <see cref="ISettings"/> used to exercise settings-dependent logic.</summary>
internal sealed class MockSettings : ISettings {
  private readonly Dictionary<string, string> _store = new();

  public bool Contains(string name) => _store.ContainsKey(name);

  public void SetValue(string name, string value) => _store[name] = value;

  public string GetValue(string name, string value) => _store.TryGetValue(name, out string? v) ? v : value;

  public void Remove(string name) => _store.Remove(name);

  public int Count => _store.Count;
}

/// <summary>Minimal <see cref="ISensor"/> stand-in for visitor and parameter tests.</summary>
internal sealed class MockSensor : ISensor {
  public MockSensor(Identifier identifier) => Identifier = identifier;

  public IControl Control => null!;
  public IHardware Hardware { get; set; } = null!;
  public Identifier Identifier { get; }
  public int Index => 0;
  public bool IsDefaultHidden => false;
  public float? Max { get; set; }
  public float? Min { get; set; }
  public string Name { get; set; } = "Mock";
  public IReadOnlyList<IParameter> Parameters { get; set; } = Array.Empty<IParameter>();
  public SensorType SensorType => SensorType.Temperature;
  public float? Value => null;
  public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
  public TimeSpan ValuesTimeWindow { get; set; }

  public void ResetMin() { }
  public void ResetMax() { }
  public void ClearValues() { }

  public void Accept(IVisitor visitor) => visitor.VisitSensor(this);
  public void Traverse(IVisitor visitor) { }
}

/// <summary>Minimal <see cref="IHardware"/> stand-in that records whether it was traversed.</summary>
internal sealed class MockHardware : IHardware {
  public bool Traversed { get; private set; }

  public HardwareType HardwareType => HardwareType.Motherboard;
  public Identifier Identifier { get; set; } = new Identifier("mock", "hardware");
  public string Name { get; set; } = "MockHardware";
  public IHardware Parent => null!;
  public ISensor[] Sensors { get; set; } = Array.Empty<ISensor>();
  public IHardware[] SubHardware => Array.Empty<IHardware>();
  public IDictionary<string, string> Properties => new Dictionary<string, string>();

  public string GetReport() => string.Empty;
  public void Update() { }

  public event SensorEventHandler SensorAdded { add { } remove { } }
  public event SensorEventHandler SensorRemoved { add { } remove { } }

  public void Accept(IVisitor visitor) => visitor.VisitHardware(this);

  public void Traverse(IVisitor visitor) {
    Traversed = true;
    foreach (ISensor sensor in Sensors)
      sensor.Accept(visitor);
  }
}

/// <summary>Minimal <see cref="IComputer"/> stand-in that records whether it was traversed.</summary>
internal sealed class MockComputer : IComputer {
  public bool Traversed { get; private set; }

  public IList<IHardware> Hardware { get; set; } = new List<IHardware>();
  public bool IsBatteryEnabled => false;
  public bool IsControllerEnabled => false;
  public bool IsCpuEnabled => false;
  public bool IsGpuEnabled => false;
  public bool IsPowerMonitorEnabled => false;
  public bool IsMemoryEnabled => false;
  public bool IsMotherboardEnabled => false;
  public bool IsNetworkEnabled => false;
  public bool IsPsuEnabled => false;
  public bool IsStorageEnabled => false;

  public string GetReport() => string.Empty;

  public event HardwareEventHandler HardwareAdded { add { } remove { } }
  public event HardwareEventHandler HardwareRemoved { add { } remove { } }

  public void Accept(IVisitor visitor) => visitor.VisitComputer(this);

  public void Traverse(IVisitor visitor) {
    Traversed = true;
    foreach (IHardware hardware in Hardware)
      hardware.Accept(visitor);
  }
}
