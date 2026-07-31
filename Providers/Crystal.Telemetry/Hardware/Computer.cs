using Crystal.Telemetry.Hardware.Battery;
using Crystal.Telemetry.Hardware.Controller.AeroCool;
using Crystal.Telemetry.Hardware.Controller.AquaComputer;
using Crystal.Telemetry.Hardware.Controller.Arctic;
using Crystal.Telemetry.Hardware.Controller.Heatmaster;
using Crystal.Telemetry.Hardware.Controller.MSI;
using Crystal.Telemetry.Hardware.Controller.Nzxt;
using Crystal.Telemetry.Hardware.Controller.Razer;
using Crystal.Telemetry.Hardware.Controller.TBalancer;
using Crystal.Telemetry.Hardware.Cpu;
using Crystal.Telemetry.Hardware.Gpu;
using Crystal.Telemetry.Hardware.Memory;
using Crystal.Telemetry.Hardware.Motherboard;
using Crystal.Telemetry.Hardware.Network;
using Crystal.Telemetry.Hardware.PowerMonitor;
using Crystal.Telemetry.Hardware.Psu.Corsair;
using Crystal.Telemetry.Hardware.Psu.Msi;
using Crystal.Telemetry.Hardware.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Crystal.Telemetry.Hardware;

/// <summary>
/// Stores all hardware groups and decides which devices should be enabled and updated.
/// </summary>
public class Computer : IComputer {
  private readonly List<IGroup> _groups = new();
  private readonly object _lock = new();
  private readonly ISettings _settings;

  private bool _batteryEnabled;
  private bool _controllerEnabled;
  private bool _cpuEnabled;
  private bool _gpuEnabled;
  private bool _powerMonitorEnabled;
  private bool _memoryEnabled;
  private bool _motherboardEnabled;
  private bool _networkEnabled;
  private bool _open;
  private bool _psuEnabled;
  private SMBios _smbios;
  private bool _storageEnabled;

  /// <summary>
  /// Creates a new <see cref="IComputer" /> instance with basic initial <see cref="Settings" />.
  /// </summary>
  public Computer() {
    _settings = new Settings();
  }

  /// <summary>
  /// Creates a new <see cref="IComputer" /> instance with additional <see cref="ISettings" />.
  /// </summary>
  /// <param name="settings">Computer settings that will be transferred to each <see cref="IHardware" />.</param>
  public Computer(ISettings settings) {
    _settings = settings ?? new Settings();
  }

  /// <inheritdoc />
  public event HardwareEventHandler HardwareAdded;

  /// <inheritdoc />
  public event HardwareEventHandler HardwareRemoved;

  /// <inheritdoc />
  public IList<IHardware> Hardware {
    get {
      lock (_lock) {
        List<IHardware> list = new();

        foreach (IGroup group in _groups)
          list.AddRange(group.Hardware);

        return list;
      }
    }
  }

  /// <inheritdoc />
  public bool IsBatteryEnabled {
    get { return _batteryEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _batteryEnabled) {
          if (value)
            AddLocked(new BatteryGroup(_settings));
          else
            RemoveTypeLocked<BatteryGroup>();
        }

        _batteryEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsControllerEnabled {
    get { return _controllerEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _controllerEnabled) {
          if (value) {
            AddLocked(new TBalancerGroup(_settings));
            AddLocked(new HeatmasterGroup(_settings));
            AddLocked(new AquaComputerGroup(_settings));
            AddLocked(new AeroCoolGroup(_settings));
            AddLocked(new NzxtGroup(_settings));
            AddLocked(new RazerGroup(_settings));
            AddLocked(new ArcticGroup(_settings));
            AddLocked(new MsiGroup(_settings));
          }
          else {
            RemoveTypeLocked<TBalancerGroup>();
            RemoveTypeLocked<HeatmasterGroup>();
            RemoveTypeLocked<AquaComputerGroup>();
            RemoveTypeLocked<AeroCoolGroup>();
            RemoveTypeLocked<NzxtGroup>();
            RemoveTypeLocked<RazerGroup>();
            RemoveTypeLocked<ArcticGroup>();
            RemoveTypeLocked<MsiGroup>();
          }
        }

        _controllerEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsCpuEnabled {
    get { return _cpuEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _cpuEnabled) {
          if (value)
            AddLocked(new CpuGroup(_settings));
          else
            RemoveTypeLocked<CpuGroup>();
        }

        _cpuEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsGpuEnabled {
    get { return _gpuEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _gpuEnabled) {
          if (value) {
            AddLocked(new AmdGpuGroup(_settings));
            AddLocked(new NvidiaGroup(_settings));

            if (_cpuEnabled)
              AddLocked(new IntelGpuGroup(GetIntelCpusLocked(), _settings));
          }
          else {
            RemoveTypeLocked<AmdGpuGroup>();
            RemoveTypeLocked<NvidiaGroup>();
            RemoveTypeLocked<IntelGpuGroup>();
          }
        }

        _gpuEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsPowerMonitorEnabled {
    get { return _powerMonitorEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _powerMonitorEnabled) {
          if (value)
            AddLocked(new PowerMonitorGroup(_settings));
          else
            RemoveTypeLocked<PowerMonitorGroup>();
        }

        _powerMonitorEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsMemoryEnabled {
    get { return _memoryEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _memoryEnabled) {
          if (value)
            AddLocked(new MemoryGroup(_settings));
          else
            RemoveTypeLocked<MemoryGroup>();
        }

        _memoryEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsMotherboardEnabled {
    get { return _motherboardEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _motherboardEnabled) {
          if (value)
            AddLocked(new MotherboardGroup(_smbios, _settings));
          else
            RemoveTypeLocked<MotherboardGroup>();
        }

        _motherboardEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsNetworkEnabled {
    get { return _networkEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _networkEnabled) {
          if (value)
            AddLocked(new NetworkGroup(_settings));
          else
            RemoveTypeLocked<NetworkGroup>();
        }

        _networkEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsPsuEnabled {
    get { return _psuEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _psuEnabled) {
          if (value) {
            AddLocked(new CorsairPsuGroup(_settings));
            AddLocked(new MsiPsuGroup(_settings));
          }
          else {
            RemoveTypeLocked<CorsairPsuGroup>();
            RemoveTypeLocked<MsiPsuGroup>();
          }
        }

        _psuEnabled = value;
      }
    }
  }

  /// <inheritdoc />
  public bool IsStorageEnabled {
    get { return _storageEnabled; }
    set {
      lock (_lock) {
        if (_open && value != _storageEnabled) {
          if (value)
            AddLocked(new StorageGroup(_settings));
          else
            RemoveTypeLocked<StorageGroup>();
        }

        _storageEnabled = value;
      }
    }
  }

  /// <summary>
  /// Contains computer information table read in accordance with <see href="https://www.dmtf.org/standards/smbios">System Management BIOS (SMBIOS) Reference Specification</see>.
  /// </summary>
  public SMBios SMBios {
    get {
      if (!_open)
        throw new InvalidOperationException("SMBIOS cannot be accessed before opening.");

      return _smbios;
    }
  }

  /// <summary>
  /// Generates a text report describing all groups, hardware, sensors and parameters currently tracked.
  /// </summary>
  /// <returns>A report describing the current state of the <see cref="Computer" />.</returns>
  public string GetReport() {
    lock (_lock) {
      using StringWriter w = new(CultureInfo.InvariantCulture);

      w.WriteLine();
      w.WriteLine(nameof(Crystal.Telemetry) + " Report");
      w.WriteLine();

      Version version = typeof(Computer).Assembly.GetName().Version;

      NewSection(w);
      w.Write("Version: ");
      w.WriteLine(version.ToString());
      w.WriteLine();

      NewSection(w);
      w.Write("Common Language Runtime: ");
      w.WriteLine(Environment.Version.ToString());
      w.Write("Operating System: ");
      w.WriteLine(Environment.OSVersion.ToString());
      w.Write("Process Type: ");
      w.WriteLine(IntPtr.Size == 4 ? "32-Bit" : "64-Bit");
      w.WriteLine();

      NewSection(w);
      w.WriteLine("Sensors");
      w.WriteLine();

      foreach (IGroup group in _groups) {
        foreach (IHardware hardware in group.Hardware)
          ReportHardwareSensorTree(hardware, w, string.Empty);
      }

      w.WriteLine();

      NewSection(w);
      w.WriteLine("Parameters");
      w.WriteLine();

      foreach (IGroup group in _groups) {
        foreach (IHardware hardware in group.Hardware)
          ReportHardwareParameterTree(hardware, w, string.Empty);
      }

      w.WriteLine();

      foreach (IGroup group in _groups) {
        string report = group.GetReport();
        if (!string.IsNullOrEmpty(report)) {
          NewSection(w);
          w.Write(report);
        }

        foreach (IHardware hardware in group.Hardware)
          ReportHardware(hardware, w);
      }

      return w.ToString();
    }
  }

  /// <summary>
  /// Triggers the <see cref="IVisitor.VisitComputer" /> method for the given observer.
  /// </summary>
  /// <param name="visitor">Observer who call to devices.</param>
  public void Accept(IVisitor visitor) {
    if (visitor == null)
      throw new ArgumentNullException(nameof(visitor));

    visitor.VisitComputer(this);
  }

  /// <summary>
  /// Triggers the <see cref="IElement.Accept" /> method with the given visitor for each device in each group.
  /// </summary>
  /// <param name="visitor">Observer who call to devices.</param>
  public void Traverse(IVisitor visitor) {
    lock (_lock) {
      // Use a for-loop instead of foreach to avoid a collection modified exception after sleep, even though everything is under a lock.
      for (int i = 0; i < _groups.Count; i++) {
        IGroup group = _groups[i];

        for (int j = 0; j < group.Hardware.Count; j++)
          group.Hardware[j].Accept(visitor);
      }
    }
  }

  private void HardwareAddedEvent(IHardware hardware) {
    HardwareAdded?.Invoke(hardware);
  }

  private void HardwareRemovedEvent(IHardware hardware) {
    HardwareRemoved?.Invoke(hardware);
  }

  /// <summary>
  /// Adds a group to <see cref="_groups"/>. Acquires <see cref="_lock"/> internally —
  /// do NOT call while already holding the lock; use <see cref="AddLocked"/> instead.
  /// </summary>
  private void Add(IGroup group) {
    if (group == null)
      return;

    List<IHardware> added;

    lock (_lock) {
      if (_groups.Contains(group))
        return;

      _groups.Add(group);

      if (group is IHardwareChanged hardwareChanged) {
        hardwareChanged.HardwareAdded += HardwareAddedEvent;
        hardwareChanged.HardwareRemoved += HardwareRemovedEvent;
      }

      added = new List<IHardware>(group.Hardware);
    }

    if (HardwareAdded != null) {
      foreach (IHardware hardware in added)
        HardwareAdded(hardware);
    }
  }

  /// <summary>
  /// Adds a group to <see cref="_groups"/>. Must be called while already holding <see cref="_lock"/>.
  /// Does NOT fire <see cref="HardwareAdded"/> events (caller is responsible if needed).
  /// </summary>
  private void AddLocked(IGroup group) {
    if (group == null || _groups.Contains(group))
      return;

    _groups.Add(group);

    if (group is IHardwareChanged hardwareChanged) {
      hardwareChanged.HardwareAdded += HardwareAddedEvent;
      hardwareChanged.HardwareRemoved += HardwareRemovedEvent;
    }
  }

  /// <summary>
  /// Removes a group and closes it. Acquires <see cref="_lock"/> internally —
  /// do NOT call while already holding the lock; use <see cref="RemoveLocked"/> instead.
  /// </summary>
  private void Remove(IGroup group) {
    List<IHardware> removed;

    lock (_lock) {
      if (!_groups.Contains(group))
        return;

      _groups.Remove(group);

      if (group is IHardwareChanged hardwareChanged) {
        hardwareChanged.HardwareAdded -= HardwareAddedEvent;
        hardwareChanged.HardwareRemoved -= HardwareRemovedEvent;
      }

      removed = new List<IHardware>(group.Hardware);
    }

    if (HardwareRemoved != null) {
      foreach (IHardware hardware in removed)
        HardwareRemoved(hardware);
    }

    group.Close();
  }

  /// <summary>
  /// Removes a group from <see cref="_groups"/> state only. Must be called while already holding <see cref="_lock"/>.
  /// Does NOT fire <see cref="HardwareRemoved"/> events or call <see cref="IGroup.Close"/>.
  /// The caller must do both after releasing the lock.
  /// </summary>
  private bool RemoveLocked(IGroup group) {
    if (!_groups.Contains(group))
      return false;

    _groups.Remove(group);

    if (group is IHardwareChanged hardwareChanged) {
      hardwareChanged.HardwareAdded -= HardwareAddedEvent;
      hardwareChanged.HardwareRemoved -= HardwareRemovedEvent;
    }

    return true;
  }

  private void RemoveType<T>() where T : IGroup {
    List<T> list = [];

    lock (_lock) {
      foreach (IGroup group in _groups) {
        if (group is T t)
          list.Add(t);
      }
    }

    foreach (T group in list)
      Remove(group);
  }

  /// <summary>
  /// Removes all groups of type <typeparamref name="T"/> from <see cref="_groups"/> state only.
  /// Must be called while already holding <see cref="_lock"/>.
  /// Returns the removed groups so the caller can close them after releasing the lock.
  /// </summary>
  private List<IGroup> RemoveTypeLocked<T>() where T : IGroup {
    List<IGroup> removed = [];

    foreach (IGroup group in _groups) {
      if (group is T)
        removed.Add(group);
    }

    foreach (IGroup group in removed)
      RemoveLocked(group);

    return removed;
  }

  /// <summary>
  /// If hasn't been opened before, opens <see cref="SMBios" />, <see cref="OpCode" /> and triggers the private <see cref="AddGroups" /> method depending on which categories are
  /// enabled.
  /// </summary>
  public void Open() {
    lock (_lock) {
      if (_open)
        return;

      _smbios = new SMBios();

      if (Software.OperatingSystem.IsWindows8OrGreater)
        Mutexes.Open();

      OpCode.Open();

      // AddGroups uses AddLocked which assumes _lock is held
      AddGroups();

      _open = true;
    }
  }

  private void AddGroups() {
    // Must be called while holding _lock. Uses AddLocked throughout.
    if (_motherboardEnabled)
      AddLocked(new MotherboardGroup(_smbios, _settings));

    if (_cpuEnabled)
      AddLocked(new CpuGroup(_settings));

    if (_memoryEnabled)
      AddLocked(new MemoryGroup(_settings));

    if (_gpuEnabled) {
      AddLocked(new AmdGpuGroup(_settings));
      AddLocked(new NvidiaGroup(_settings));

      if (_cpuEnabled)
        AddLocked(new IntelGpuGroup(GetIntelCpusLocked(), _settings));
    }

    if (_powerMonitorEnabled)
      AddLocked(new PowerMonitorGroup(_settings));

    if (_controllerEnabled) {
      AddLocked(new TBalancerGroup(_settings));
      AddLocked(new HeatmasterGroup(_settings));
      AddLocked(new AquaComputerGroup(_settings));
      AddLocked(new AeroCoolGroup(_settings));
      AddLocked(new NzxtGroup(_settings));
      AddLocked(new RazerGroup(_settings));
      AddLocked(new ArcticGroup(_settings));
      AddLocked(new MsiGroup(_settings));
    }

    if (_storageEnabled)
      AddLocked(new StorageGroup(_settings));

    if (_networkEnabled)
      AddLocked(new NetworkGroup(_settings));

    if (_psuEnabled) {
      AddLocked(new CorsairPsuGroup(_settings));
      AddLocked(new MsiPsuGroup(_settings));
    }

    if (_batteryEnabled)
      AddLocked(new BatteryGroup(_settings));
  }

  private static void NewSection(TextWriter writer) {
    for (int i = 0; i < 8; i++)
      writer.Write("----------");

    writer.WriteLine();
    writer.WriteLine();
  }

  private static int CompareSensor(ISensor a, ISensor b) {
    int c = a.SensorType.CompareTo(b.SensorType);
    if (c == 0)
      return a.Index.CompareTo(b.Index);

    return c;
  }

  private static void ReportHardwareSensorTree(IHardware hardware, TextWriter w, string space) {
    w.WriteLine("{0}|", space);
    w.WriteLine("{0}+- {1} ({2})", space, hardware.Name, hardware.Identifier);

    ISensor[] sensors = hardware.Sensors;
    Array.Sort(sensors, CompareSensor);

    foreach (ISensor sensor in sensors)
      w.WriteLine("{0}|  +- {1,-14} : {2,8:G6} {3,8:G6} {4,8:G6} ({5})", space, sensor.Name, sensor.Value, sensor.Min, sensor.Max, sensor.Identifier);

    foreach (IHardware subHardware in hardware.SubHardware)
      ReportHardwareSensorTree(subHardware, w, "|  ");
  }

  private static void ReportHardwareParameterTree(IHardware hardware, TextWriter w, string space) {
    w.WriteLine("{0}|", space);
    w.WriteLine("{0}+- {1} ({2})", space, hardware.Name, hardware.Identifier);

    ISensor[] sensors = hardware.Sensors;
    Array.Sort(sensors, CompareSensor);

    foreach (ISensor sensor in sensors) {
      string innerSpace = space + "|  ";
      if (sensor.Parameters.Count > 0) {
        w.WriteLine("{0}|", innerSpace);
        w.WriteLine("{0}+- {1} ({2})", innerSpace, sensor.Name, sensor.Identifier);

        foreach (IParameter parameter in sensor.Parameters) {
          string innerInnerSpace = innerSpace + "|  ";
          w.WriteLine("{0}+- {1} : {2}", innerInnerSpace, parameter.Name, string.Format(CultureInfo.InvariantCulture, "{0} : {1}", parameter.DefaultValue, parameter.Value));
        }
      }
    }

    foreach (IHardware subHardware in hardware.SubHardware)
      ReportHardwareParameterTree(subHardware, w, "|  ");
  }

  private static void ReportHardware(IHardware hardware, TextWriter w) {
    string hardwareReport = hardware.GetReport();
    if (!string.IsNullOrEmpty(hardwareReport)) {
      NewSection(w);
      w.Write(hardwareReport);
    }

    foreach (IHardware subHardware in hardware.SubHardware)
      ReportHardware(subHardware, w);
  }

  /// <summary>
  /// If opened before, removes all <see cref="IGroup" /> and triggers <see cref="OpCode.Close" />.
  /// </summary>
  public void Close() {
    List<IGroup> groupsToClose;

    lock (_lock) {
      if (!_open)
        return;

      // Snapshot and unregister all groups under the lock
      groupsToClose = new List<IGroup>(_groups);
      _groups.Clear();

      foreach (IGroup group in groupsToClose) {
        if (group is IHardwareChanged hardwareChanged) {
          hardwareChanged.HardwareAdded -= HardwareAddedEvent;
          hardwareChanged.HardwareRemoved -= HardwareRemovedEvent;
        }
      }

      OpCode.Close();
      Mutexes.Close();

      _smbios = null;
      _open = false;
    }

    // Fire HardwareRemoved events and close groups outside the lock
    // to avoid deadlocks if event handlers call back into Computer.
    foreach (IGroup group in groupsToClose) {
      if (HardwareRemoved != null) {
        foreach (IHardware hardware in group.Hardware)
          HardwareRemoved(hardware);
      }

      group.Close();
    }
  }

  /// <summary>
  /// If opened before, removes all <see cref="IGroup" /> and recreates it.
  /// </summary>
  public void Reset() {
    lock (_lock) {
      if (!_open)
        return;
    }

    RemoveGroups();

    lock (_lock) {
      AddGroups();
    }
  }

  private void RemoveGroups() {
    List<IGroup> groupsToClose;

    lock (_lock) {
      groupsToClose = new List<IGroup>(_groups);
      _groups.Clear();

      foreach (IGroup group in groupsToClose) {
        if (group is IHardwareChanged hardwareChanged) {
          hardwareChanged.HardwareAdded -= HardwareAddedEvent;
          hardwareChanged.HardwareRemoved -= HardwareRemovedEvent;
        }
      }
    }

    foreach (IGroup group in groupsToClose) {
      if (HardwareRemoved != null) {
        foreach (IHardware hardware in group.Hardware)
          HardwareRemoved(hardware);
      }

      group.Close();
    }
  }

  /// <summary>
  /// Must be called while already holding <see cref="_lock"/>.
  /// </summary>
  private List<IntelCpu> GetIntelCpusLocked() {
    IGroup cpuGroup = _groups.Find(x => x is CpuGroup) ?? new CpuGroup(_settings);
    return cpuGroup.Hardware.Select(x => x as IntelCpu).ToList();
  }

  /// <summary>
  /// <see cref="Computer" /> specific additional settings passed to its <see cref="IHardware" />.
  /// </summary>
  private class Settings : ISettings {
    public bool Contains(string name) {
      return false;
    }

    public void SetValue(string name, string value) { }

    public string GetValue(string name, string value) {
      return value;
    }

    public void Remove(string name) { }
  }
}