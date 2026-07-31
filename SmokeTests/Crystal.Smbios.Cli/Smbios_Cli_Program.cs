using Crystal.Smbios;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using System;
using System.Linq;

internal class Smbios_Cli_Program {
  static void generate_dump_file() {
    var (tableData, major, minor) = WindowsSmbiosReader.ReadTableData();
    var outPath = Path.Combine(Directory.GetCurrentDirectory(), "smbios_dump.bin");
    File.WriteAllBytes(outPath, tableData);
    File.WriteAllText(Path.ChangeExtension(outPath, ".meta"), $"{major}.{minor}");
    Console.WriteLine($"Wrote: {outPath} ({tableData.Length} bytes)");
    Console.WriteLine($"Wrote: {Path.ChangeExtension(outPath, ".meta")} (SMBIOS {major}.{minor})");
  }

  static void TestType15() {
    var table = SmbiosTable.Load();
    // Debug: show raw Type 15 structures and try a manual decode
    var raw15 = table.RawStructures.Where(s => s.Type == (SmbiosStructureType)15).ToList();
    Console.WriteLine($"\nRaw Type 15 structures: {raw15.Count}");
    
    foreach (var r in raw15) {
      Console.WriteLine($"  Handle: 0x{r.Handle:X4}  Length: {r.Length}  Strings: {r.Strings.Count}");
      Console.WriteLine("  Strings:");
      foreach (var s in r.Strings) Console.WriteLine($"    - {s}");
      var span = r.FormattedArea.Span;
      if (r.Length > 0x05) {
        ushort logAreaLength = (ushort)(span[0x04] | (span[0x05] << 8));
        Console.WriteLine($"  LogAreaLength: {logAreaLength}");
      }
      else Console.WriteLine("  Formatted area too short to read LogAreaLength");
      Console.WriteLine($"  Formatted bytes: {BitConverter.ToString(r.FormattedArea.ToArray())}");
    }

    // Manual parse of formatted-area offsets (defensive)
    foreach (var r in raw15) {
      var span = r.FormattedArea.Span;
      ushort logAreaLength = span.Length > 5 ? (ushort)(span[4] | (span[5] << 8)) : (ushort)0;
      ushort headerOffset = span.Length > 7 ? (ushort)(span[6] | (span[7] << 8)) : (ushort)0;
      byte headerFormat = span.Length > 8 ? span[8] : (byte)0;
      byte headerLen = span.Length > 9 ? span[9] : (byte)0;
      Console.WriteLine($"Manual decode: LogLen={logAreaLength} HeaderOff={headerOffset} Format={headerFormat} HdrLen={headerLen}");
    }
  }

  static void quickDiag() {
    // Insert (or run) to list counts per SMBIOS type
    var table = SmbiosTable.Load();
    var grouped = table.RawStructures.GroupBy(s => s.Type)
                    .OrderBy(g => (byte)g.Key)
                    .Select(g => ($"{g.Key} ({(byte)g.Key})", g.Count()));
    foreach (var g in grouped) Console.WriteLine($"{g.Item1}: {g.Item2}");

    var bytes = System.IO.File.ReadAllBytes(@"C:\path\to\smbios_dump.bin");
    var table1 = SmbiosTable.FromRawTableData(bytes, majorVersion: 3, minorVersion: 2);
    Console.WriteLine($"Type 15 raw count: {table1.RawStructures.Count(s => s.Type == (SmbiosStructureType)15)}");
  }

  static void TestType15_2() {
    var bytes = File.ReadAllBytes(@"C:\path\to\smbios_dump.bin");
    var versionText = File.ReadAllText(@"C:\path\to\smbios_dump.meta").Trim().Split('.');
    byte major = byte.Parse(versionText[0]);
    byte minor = byte.Parse(versionText[1]);
    var table = SmbiosTable.FromRawTableData(bytes, major, minor);
  }

  static void GeneralTest() {
    try {
      var table = SmbiosTable.Load();
      Console.WriteLine($"SMBIOS Version: {table.MajorVersion}.{table.MinorVersion}");
      Console.WriteLine($"Total Structures: {table.RawStructures.Count}\n");

      // Summary: BIOS / System / Baseboard / Chassis
      Console.WriteLine("=== Summary ===");
      Console.WriteLine($"BIOS: {table.Bios?.Vendor ?? "(unknown)"} {table.Bios?.Version ?? ""} ({table.Bios?.ReleaseDate ?? ""})");
      Console.WriteLine($"System: {table.System?.Manufacturer ?? "(unknown)"} - {table.System?.ProductName ?? ""} {table.System?.Version ?? ""}  Serial: {table.System?.SerialNumber ?? "(none)"}");
      Console.WriteLine($"Baseboard: {table.Baseboard?.Manufacturer ?? "(unknown)"} - {table.Baseboard?.Product ?? ""}  Serial: {table.Baseboard?.SerialNumber ?? "(none)"}");
      Console.WriteLine($"Chassis: {table.Chassis?.Manufacturer ?? "(unknown)"}  TypeRaw: {table.Chassis?.ChassisType}");
      Console.WriteLine();
      // Additional quick stats
      Console.WriteLine("=== Quick Stats ===");
      Console.WriteLine($"Processors: {table.ProcessorInformation.Count} (Populated: {table.PopulatedProcessors.Count()})");
      Console.WriteLine($"Total logical cores: {table.ProcessorInformation.Sum(p => p.LogicalCoreCount)}  Total logical threads: {table.ProcessorInformation.Sum(p => p.LogicalThreadCount)}");
      Console.WriteLine($"First OEM string: {table.FirstOemString ?? "(none)"}");
      Console.WriteLine($"Total cache installed: {table.CacheInformation.Sum(c => c.InstalledSizeKiB)} KiB  Max supported: {table.CacheInformation.Sum(c => c.MaxSizeKiB)} KiB");
      Console.WriteLine();

      // Cache Information
      Console.WriteLine("=== Cache Information ===");
      if (table.CacheInformation.Count == 0)
        Console.WriteLine("  (No cache information available)");
      else {
        foreach (var cache in table.CacheInformation) {
          Console.WriteLine($"  Handle: {cache.Handle}");
          Console.WriteLine($"    Max Size: {cache.MaxSizeKiB} KiB");
          Console.WriteLine($"    Installed: {cache.InstalledSizeKiB} KiB");
          Console.WriteLine($"    Type: {cache.SystemCacheType}");
          Console.WriteLine($"    Level: {cache.Configuration.Level}");
          Console.WriteLine();
        }
      }

      // Memory Error Information (Type 18 / 33)
      Console.WriteLine("\n=== Memory Error Information ===");
      if (table.MemoryErrorEntries.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var entry in table.MemoryErrorEntries) {
          var info = entry.Info;
          Console.WriteLine($"  ErrorType: 0x{info.ErrorType:X2}  Granularity: 0x{info.ErrorGranularity:X2}  Operation: 0x{info.ErrorOperation:X2}  Is64: {info.Is64Bit}");
          Console.WriteLine($"    VendorSyndrome: 0x{info.VendorSyndrome:X}");
          Console.WriteLine($"    MemoryArrayErrorAddress: {FormatHexWithSep(info.MemoryArrayErrorAddress, info.Is64Bit ? 8 : 4)}");
          Console.WriteLine($"    DeviceErrorAddress: {FormatHexWithSep(info.DeviceErrorAddress, info.Is64Bit ? 8 : 4)}");
          Console.WriteLine($"    ErrorResolution: {FormatHexWithSep(info.ErrorResolution, 4)}");
        }
      }

      // Built-in Pointing Devices (Type 21)
      Console.WriteLine("\n=== Built-in Pointing Devices (Type 21) ===");
      if (table.BuiltInPointingDevices.Count == 0)
        Console.WriteLine("  (No pointing devices found)");
      else {
        foreach (var device in table.BuiltInPointingDevices) {
          Console.WriteLine($"  Device Type: {device.DeviceType}");
          Console.WriteLine($"    Interface: {device.Interface}");
          Console.WriteLine($"    Number of Buttons: {device.NumberOfButtons}");
          Console.WriteLine();
        }
      }

      // Onboard Devices
      Console.WriteLine("\n=== Onboard Devices (All) ===");
      if (table.OnboardDevices.Count == 0)
        Console.WriteLine("  (No onboard devices found in SMBIOS table)");
      else {
        foreach (var device in table.OnboardDevices) {
          Console.WriteLine($"  {device.ReferenceDesignation ?? "(unnamed)"}");
          Console.WriteLine($"    Type: {device.DeviceType}");
          Console.WriteLine($"    Enabled: {device.IsEnabled}");
          Console.WriteLine($"    PCI: {device.SegmentGroupNumber}:{device.BusNumber:X2}:{device.DeviceNumber}.{device.FunctionNumber}");
          Console.WriteLine();
        }
      }

      // Enabled Onboard Devices
      Console.WriteLine("\n=== Enabled Onboard Devices ===");
      var enabledDevices = table.EnabledOnboardDevices.ToList();
      if (enabledDevices.Count == 0)
        Console.WriteLine("  (No enabled onboard devices found)");
      else {
        foreach (var device in enabledDevices) {
          Console.WriteLine($"  {device.ReferenceDesignation ?? "(unnamed)"}: {device.DeviceType}");
        }
      }
      var type41Structures = table.RawStructures.Where(s => s.Type == (SmbiosStructureType)41).ToList();
      Console.WriteLine($"Type 41 structures found: {type41Structures.Count}");

      // Port Connectors
      Console.WriteLine("\n=== Port Connectors ===");
      if (table.PortConnectors.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var port in table.PortConnectors) {
          Console.WriteLine($"  {port.ExternalReferenceDesignator ?? "(unnamed)"}: {port.PortType}, {port.ExternalConnectorType}");
        }
      }

      // System Slots
      Console.WriteLine("\n=== System Slots ===");
      if (table.SystemSlots.Count == 0)
        Console.WriteLine("  (none reported)");
      else
        foreach (var slot in table.SystemSlots)
          Console.WriteLine($"  {slot.SlotDesignation ?? "(unnamed)"}: {slot.SlotType}, {slot.DataBusWidth}, {(slot.IsInUse ? "IN USE" : "empty")}");

      // Physical Memory Arrays
      Console.WriteLine("\n=== Physical Memory Arrays ===");
      foreach (var array in table.PhysicalMemoryArrays) {
        Console.WriteLine($"  Location: {array.Location}, Use: {array.Use}, Max: {array.MaxCapacityKiB / 1024 / 1024} GiB, Slots: {array.NumberOfMemoryDevices}");

        foreach (var dimm in table.GetDevicesIn(array)) {
          // Calculate size in MiB safely using our updated CapacityBytes property
          string sizeText = dimm.IsPopulated
            ? $"{dimm.CapacityBytes / (1024L * 1024L)}"
            : "empty";

          // Use .Type instead of .MemoryType, and .SpeedMts instead of .Speed
          Console.WriteLine($"    {dimm.DeviceLocator ?? "(unknown slot)"}: {sizeText} MiB, {dimm.Type}, {dimm.SpeedMts} MT/s");
        }
      }

      Console.WriteLine($"\nTotal Installed Memory: {table.TotalInstalledMemoryMiB} MiB ({table.TotalInstalledMemoryMiB / 1024.0:F1} GiB)");

      // Memory mapped addresses (Type 19 / 20) - illustrate details
      Console.WriteLine("\n=== Memory Mapped Addresses (Type 19) ===");
      if (table.MemoryArrayMappedAddresses.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var m in table.MemoryArrayMappedAddresses) {
          Console.WriteLine($"  ArrayHandle: 0x{m.MemoryArrayHandle:X4}  Start: 0x{m.StartAddressBytes:X}  End: 0x{m.EndAddressBytes:X}");
          Console.WriteLine($"    StartKiB: {m.StartAddressKiB}  EndKiB: {m.EndAddressKiB}  SizeKiB: {m.SizeKiB}");
          Console.WriteLine($"    UsesExtended: {m.UsesExtendedAddresses}  PartitionWidth: {m.PartitionWidth}  IsInterleaved: {m.IsInterleaved}");
        }
      }

      Console.WriteLine("\n=== Memory Device Mapped Addresses (Type 20) ===");
      if (table.MemoryDeviceMappedAddresses.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var md in table.MemoryDeviceMappedAddresses) {
          Console.WriteLine($"  DeviceHandle: 0x{md.MemoryDeviceHandle:X4}  ArrayMappedHandle: 0x{md.MemoryArrayMappedAddressHandle:X4}  Start: 0x{md.StartAddressBytes:X}  End: 0x{md.EndAddressBytes:X}");
          Console.WriteLine($"    StartKiB: {md.StartAddressKiB}  EndKiB: {md.EndAddressKiB}  SizeKiB: {md.SizeKiB}");
          Console.WriteLine($"    UsesExtended: {md.UsesExtendedAddresses}  PartitionRowPosition: {md.PartitionRowPosition}");
          Console.WriteLine($"    InterleavePosition: {md.InterleavePosition}  InterleavedDataDepth: {md.InterleavedDataDepth}");
        }
      }

      // Cooling Devices (Type 27)
      Console.WriteLine("\n=== Cooling Devices (Type 27) ===");
      if (table.CoolingDevices.Count == 0) {
        Console.WriteLine("  (No cooling devices found)");
      }
      else {
        foreach (var device in table.CoolingDevices) {
          // 1. Check description natively parsed by your Decode method
          string description = device.Description ?? "(unnamed)";
          Console.WriteLine($"  Description: {description}");

          // 2. Display extracted physical device type and operational status enums
          Console.WriteLine($"    Device Type: {device.DeviceType}");
          Console.WriteLine($"    Operational Status: {device.Status}");
          Console.WriteLine($"    Cooling Unit Group: {device.CoolingUnitGroup}");

          // 3. Print nominal speed, handling the 0x80000000 (Unknown) marker flag safely
          if (device.IsSpeedIdentifiable) {
            Console.WriteLine($"    Nominal Speed: {device.NominalSpeedRpm} RPM");
          }
          else {
            Console.WriteLine("    Nominal Speed: (Unknown/Unsupported)");
          }

          // 4. Print probe attachment state
          if (device.HasAssociatedProbe) {
            Console.WriteLine($"    Associated Temp Probe Handle: 0x{device.TemperatureProbeHandle:X4}");
          }
          else {
            Console.WriteLine("    Associated Temp Probe Handle: None");
          }

          Console.WriteLine();
        }
      }


      // Temperature Probes (Type 28)
      Console.WriteLine("\n=== Temperature Probes (Type 28) ===");
      if (table.TemperatureProbes.Count == 0) {
        Console.WriteLine("  (No temperature probes found)");
      }
      else {
        // Loop through your list of parsed T028_TemperatureProbeInformation items directly
        foreach (var probe in table.TemperatureProbes) {
          // 1. Check description natively parsed by your Decode method
          string description = probe.Description ?? "(unnamed)";
          Console.WriteLine($"  Description: {description}");

          // 2. Display extracted structural Location and runtime Status enums
          Console.WriteLine($"    Location: {probe.Location} (0x{probe.LocationAndStatusRaw & 0x1F:X2})");
          Console.WriteLine($"    Status: {probe.Status}");

          // 3. Print temperature values converting raw 1/10th °C ints into user-friendly nullable doubles
          if (probe.NominalValueCelsius.HasValue) {
            Console.WriteLine($"    Nominal Temperature: {probe.NominalValueCelsius.Value:F1}°C");
          }
          else {
            Console.WriteLine("    Nominal Temperature: (Unknown/Unsupported)");
          }

          if (probe.MaximumValueCelsius.HasValue && probe.MinimumValueCelsius.HasValue) {
            Console.WriteLine($"    Operating Range: {probe.MinimumValueCelsius.Value:F1}°C to {probe.MaximumValueCelsius.Value:F1}°C");
          }

          Console.WriteLine();
        }
      }


      // System Power Supplies (Type 39)
      Console.WriteLine("\n=== System Power Supplies (Type 39) ===");
      if (table.PowerSupplies.Count == 0)
        Console.WriteLine("  (No power supplies found)");
      else {
        foreach (var psu in table.PowerSupplies) {
          Console.WriteLine($"  Device Name: {psu.DeviceName ?? "(unnamed)"} ({psu.Manufacturer ?? "unknown mfr"})");
          Console.WriteLine($"    Power Unit Group: {psu.PowerUnitGroup}  Location: {psu.Location ?? "(none)"}");
          Console.WriteLine($"    Model/Part: {psu.ModelPartNumber ?? "(none)"}  Rev: {psu.RevisionLevel ?? "(none)"}  Serial: {psu.SerialNumber ?? "(none)"}  Asset: {psu.AssetTagNumber ?? "(none)"}");
          Console.WriteLine($"    Type: {psu.SupplyType}  Status: {psu.Status}  InputVoltageRangeSwitching: {psu.InputVoltageRangeSwitching}");
          Console.WriteLine($"    Present: {psu.IsPresent}  HotReplaceable: {psu.IsHotReplaceable}  Unplugged: {psu.IsUnplugged}");
          Console.WriteLine($"    Max Power Capacity: {(psu.IsMaxPowerKnown ? $"{psu.MaxPowerCapacityWatts} W" : "(unknown)")}");
          Console.WriteLine($"    Probes: inputVoltage=0x{psu.InputVoltageProbeHandle:X4} cooling=0x{psu.CoolingDeviceHandle:X4} inputCurrent=0x{psu.InputCurrentProbeHandle:X4}");
          Console.WriteLine();
        }
      }

      // Portable Batteries
      Console.WriteLine("\n=== Portable Batteries ===");
      if (table.Batteries.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var battery in table.Batteries) {
          Console.WriteLine($"  {battery.DeviceName ?? "(unnamed)"} ({battery.Manufacturer ?? "unknown mfr"})");
          Console.WriteLine($"    Location: {battery.Location}");
          Console.WriteLine($"    Chemistry: {battery.DeviceChemistry}" +
                             (battery.SbdsDeviceChemistry is not null ? $" ({battery.SbdsDeviceChemistry})" : ""));
          Console.WriteLine($"    Design Capacity: {battery.DesignCapacityMilliwattHours?.ToString() ?? "unknown"} mWh");
          Console.WriteLine($"    Design Voltage: {battery.DesignVoltageMv} mV");
          Console.WriteLine($"    Serial Number: {battery.SerialNumber ?? "(none)"}");
          Console.WriteLine($"    Manufacture Date: {battery.ManufactureDate ?? battery.SbdsManufactureDate?.ToString() ?? "unknown"}");
          Console.WriteLine();
        }
      }

      DumpNewlyAddedTypes(table);
    }
    catch (Exception ex) {
      Console.WriteLine($"Error: {ex.Message}");
      Console.WriteLine(ex.StackTrace);
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }

  // Demo output for the types added on top of the original five-file SMBIOS
  // implementation: Types 5, 6, 10, 12, 14, 23-25, 29-32, 34-38, 40, 42-46.
  private static void DumpNewlyAddedTypes(SmbiosTable table) {
    Console.WriteLine("\n=== Memory Controllers (Type 5, Obsolete) ===");
    if (table.MemoryControllers.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var mc in table.MemoryControllers) {
      Console.WriteLine($"  Error Detecting: {mc.ErrorDetectingMethod}  Correcting: {mc.ErrorCorrectingCapability}");
      Console.WriteLine($"    Interleave: supported={mc.SupportedInterleave} current={mc.CurrentInterleave}");
      Console.WriteLine($"    Max Module Size: {mc.MaximumMemoryModuleSizeMiB?.ToString() ?? "unknown"} MiB  Slots: {mc.AssociatedMemorySlotCount}");
    }

    Console.WriteLine("\n=== Memory Modules (Type 6, Obsolete) ===");
    if (table.MemoryModules.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var mm in table.MemoryModules) {
      Console.WriteLine($"  {mm.SocketDesignation ?? "(unnamed)"}: {mm.CurrentMemoryType}, installed={mm.InstalledSizeMiB?.ToString() ?? "n/a"} MiB, enabled={mm.EnabledSizeMiB?.ToString() ?? "n/a"} MiB");
    }

    Console.WriteLine("\n=== On Board Devices (Type 10, Obsolete) ===");
    if (table.LegacyOnBoardDevices.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var obd in table.LegacyOnBoardDevices) {
      foreach (var d in obd.Devices)
        Console.WriteLine($"  [{(d.IsEnabled ? "enabled" : "disabled")}] {d.DeviceType}: {d.Description ?? "(unnamed)"}");
    }

    Console.WriteLine("\n=== System Configuration Options (Type 12) ===");
    if (table.SystemConfigurationOptions.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var sco in table.SystemConfigurationOptions)
      foreach (var opt in sco.Options) Console.WriteLine($"  - {opt}");

    Console.WriteLine("\n=== Group Associations (Type 14) ===");
    if (table.GroupAssociations.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var ga in table.GroupAssociations) {
      Console.WriteLine($"  {ga.GroupName ?? "(unnamed group)"}: {ga.Items.Count} member(s)");
      foreach (var item in ga.Items) Console.WriteLine($"    - {item.ItemType} @ handle 0x{item.ItemHandle:X4}");
    }

    Console.WriteLine("\n=== System Reset (Type 23) ===");
    if (table.SystemResets.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var sr in table.SystemResets) {
      Console.WriteLine($"  Enabled: {sr.IsEnabled}  Watchdog: {sr.HasWatchdogTimer}");
      Console.WriteLine($"    Boot Option: {sr.BootOption}  On Limit: {sr.BootOptionOnLimit}");
      Console.WriteLine($"    Reset Count: {sr.ResetCount}  Limit: {sr.ResetLimit}  Timer: {sr.TimerIntervalMinutes} min  Timeout: {sr.TimeoutMinutes} min");
    }

    Console.WriteLine("\n=== Hardware Security (Type 24) ===");
    var hwSec = table.HardwareSecurity;
    if (hwSec is null) Console.WriteLine("  (none reported)");
    else {
      Console.WriteLine($"  Power-On Password: {hwSec.PowerOnPasswordStatus}");
      Console.WriteLine($"  Keyboard Password: {hwSec.KeyboardPasswordStatus}");
      Console.WriteLine($"  Administrator Password: {hwSec.AdministratorPasswordStatus}");
      Console.WriteLine($"  Front Panel Reset: {hwSec.FrontPanelResetStatus}");
    }

    Console.WriteLine("\n=== System Power Controls (Type 25) ===");
    var pwrCtl = table.PowerControls;
    if (pwrCtl is null) Console.WriteLine("  (none reported)");
    else Console.WriteLine($"  Next Scheduled Power-On: month={pwrCtl.Month?.ToString() ?? "every"} day={pwrCtl.DayOfMonth?.ToString() ?? "every"} {pwrCtl.Hour:D2}:{pwrCtl.Minute:D2}:{pwrCtl.Second:D2}");

    Console.WriteLine("\n=== Electrical Current Probes (Type 29) ===");
    if (table.ElectricalCurrentProbes.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var ecp in table.ElectricalCurrentProbes) {
      Console.WriteLine($"  {ecp.Description ?? "(unnamed)"} @ {ecp.Location}: {ecp.Status}");
      Console.WriteLine($"    Max: {ecp.MaximumValueMilliamps} mA  Min: {ecp.MinimumValueMilliamps} mA  Nominal: {(ecp.IsNominalValueIdentifiable ? ecp.NominalValueMilliamps.ToString() : "unknown")} mA");
    }

    Console.WriteLine("\n=== Out-of-Band Remote Access (Type 30) ===");
    if (table.OutOfBandRemoteAccess.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var oob in table.OutOfBandRemoteAccess)
      Console.WriteLine($"  {oob.ManufacturerName ?? "(unnamed)"}: inbound={oob.InboundConnectionEnabled} outbound={oob.OutboundConnectionEnabled}");

    Console.WriteLine("\n=== Boot Integrity Services Entry Point (Type 31) ===");
    if (table.BootIntegrityServicesEntryPoints.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var bis in table.BootIntegrityServicesEntryPoints)
      Console.WriteLine($"  16-bit entry: {bis.BisEntry16Segment:X4}:{bis.BisEntry16Offset:X4}  32-bit entry: 0x{bis.BisEntry32Address:X8}");

    Console.WriteLine("\n=== System Boot Information (Type 32) ===");
    var boot = table.BootInformation;
    if (boot is null) Console.WriteLine("  (none reported)");
    else Console.WriteLine($"  Status: {(object?)boot.Status ?? $"raw 0x{boot.BootStatusRaw:X2} (OEM/product-specific)"}");

    Console.WriteLine("\n=== Management Devices (Type 34/35/36) ===");
    if (table.ManagementDevices.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var md in table.ManagementDevices)
      Console.WriteLine($"  {md.Description ?? "(unnamed)"}: {md.Type} @ 0x{md.Address:X} ({md.AddressType})");
    foreach (var comp in table.ManagementDeviceComponents)
      Console.WriteLine($"  Component {comp.Description ?? "(unnamed)"}: device=0x{comp.ManagementDeviceHandle:X4} target=0x{comp.ComponentHandle:X4} threshold={(comp.HasThreshold ? $"0x{comp.ThresholdHandle:X4}" : "none")}");
    foreach (var th in table.ManagementDeviceThresholds)
      Console.WriteLine($"  Thresholds: nonCritical=[{th.LowerThresholdNonCritical},{th.UpperThresholdNonCritical}] critical=[{th.LowerThresholdCritical},{th.UpperThresholdCritical}] nonRecoverable=[{th.LowerThresholdNonRecoverable},{th.UpperThresholdNonRecoverable}]");

    Console.WriteLine("\n=== Memory Channels (Type 37) ===");
    if (table.MemoryChannels.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var mch in table.MemoryChannels) {
      Console.WriteLine($"  {mch.ChannelType}: max load={mch.MaximumChannelLoad} total load={mch.TotalLoad}");
      foreach (var dev in mch.Devices) Console.WriteLine($"    - handle 0x{dev.MemoryDeviceHandle:X4} load {dev.DeviceLoad}");
    }

    Console.WriteLine("\n=== IPMI Device (Type 38) ===");
    var ipmi = table.Ipmi;
    if (ipmi is null) Console.WriteLine("  (none reported)");
    else Console.WriteLine($"  {ipmi.InterfaceType} v{ipmi.IpmiSpecificationMajor}.{ipmi.IpmiSpecificationMinor}  I2C: 0x{ipmi.I2CSlaveAddress:X2}  Base: 0x{ipmi.BaseAddress:X}");

    Console.WriteLine("\n=== Additional Information (Type 40) ===");
    if (table.AdditionalInformation.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var ai in table.AdditionalInformation)
      foreach (var entry in ai.Entries)
        Console.WriteLine($"  {entry.EntryString ?? "(unnamed)"}: handle=0x{entry.ReferencedHandle:X4} offset=0x{entry.ReferencedOffset:X2} value=[{string.Join(",", entry.Value.Select(b => $"0x{b:X2}"))}]");

    Console.WriteLine("\n=== Management Controller Host Interfaces (Type 42) ===");
    if (table.ManagementControllerHostInterfaces.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var mchi in table.ManagementControllerHostInterfaces) {
      Console.WriteLine($"  {(object?)mchi.InterfaceType ?? $"raw 0x{mchi.InterfaceTypeRaw:X2}"}: {mchi.InterfaceTypeSpecificData.Count} spec byte(s), {mchi.ProtocolRecordCount} protocol record(s)");
      foreach (var rec in mchi.ProtocolRecords)
        Console.WriteLine($"    - Protocol: {(object?)rec.ProtocolType ?? $"raw 0x{rec.ProtocolTypeRaw:X2}"} ({rec.ProtocolTypeSpecificDataLength} data byte(s))");
    }

    Console.WriteLine("\n=== TPM Device (Type 43) ===");
    var tpm = table.Tpm;
    if (tpm is null) Console.WriteLine("  (none reported)");
    else Console.WriteLine($"  {tpm.VendorId} spec {tpm.MajorSpecVersion}.{tpm.MinorSpecVersion}  {tpm.Description ?? "(unnamed)"}  Characteristics: {tpm.Characteristics}");

    Console.WriteLine("\n=== Processor Additional Information (Type 44) ===");
    if (table.ProcessorAdditionalInformation.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var pai in table.ProcessorAdditionalInformation)
      Console.WriteLine($"  Processor 0x{pai.ReferencedHandle:X4}: {pai.ProcessorArchitectureType}, {pai.ProcessorSpecificData.Count} data byte(s)");

    Console.WriteLine("\n=== Firmware Inventory (Type 45) ===");
    if (table.FirmwareInventory.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var fw in table.FirmwareInventory)
      Console.WriteLine($"  {fw.FirmwareComponentName ?? "(unnamed)"} v{fw.FirmwareVersion ?? "?"} ({fw.Manufacturer ?? "unknown mfr"}): {fw.State}, {fw.ImageSizeBytes} bytes");

    Console.WriteLine("\n=== String Properties (Type 46) ===");
    if (table.StringProperties.Count == 0) Console.WriteLine("  (none reported)");
    foreach (var sp in table.StringProperties)
      Console.WriteLine($"  {(object?)sp.PropertyId ?? $"raw 0x{sp.PropertyIdRaw:X4}"} -> \"{sp.PropertyValue ?? "(none)"}\" (parent 0x{sp.ParentHandle:X4})");
  }


  static void Main(string[] args) {
    //tryit();
    GeneralTest();
    //TestType15();
    //quickDiag();
    //generate_dump_file();
    //TestType15_2();
    //Run(@"C:\path\to\smbios_dump.bin", @"C:\path\to\smbios_dump.meta");
    //TestType15Synthetic.Run();
    //RunSyntheticType15Test();
  }


  public static void Run(string dumpPath, string metaPath) {
    var bytes = File.ReadAllBytes(dumpPath);
    Console.WriteLine($"File: {dumpPath}  Length: {bytes.Length} bytes");

    // Show first bytes for quick sanity check (ASCII / signature)
    Console.WriteLine("First 64 bytes (hex/ascii):");
    Console.WriteLine(BitConverter.ToString(bytes.Take(64).ToArray()));
    Console.WriteLine(System.Text.Encoding.ASCII.GetString(bytes.Take(64).ToArray()));

    // Try parsing at multiple offsets to find the structure table start.
    for (int off = 0; off <= 127; off++) {
      try {
        var raw = SmbiosTableParser.Parse(bytes.AsSpan(off));
        if (raw.Count == 0) continue;

        Console.WriteLine($"\nParse succeeded at offset {off}: {raw.Count} structures");
        var grouped = raw.GroupBy(r => r.Type)
                         .OrderBy(g => (byte)g.Key)
                         .Select(g => $"{g.Key} ({(byte)g.Key}): {g.Count()}");
        foreach (var g in grouped) Console.WriteLine($"  {g}");

        // If Type 15 present, dump details
        var found15 = raw.Where(r => r.Type == (SmbiosStructureType)15).ToList();
        Console.WriteLine($"  Type15 count at offset {off}: {found15.Count}");
        foreach (var r in found15) {
          Console.WriteLine($"    Handle: 0x{r.Handle:X4} Length: {r.Length} Strings: {r.Strings.Count}");
          Console.WriteLine($"    Formatted: {BitConverter.ToString(r.FormattedArea.ToArray())}");
          if (r.Strings.Count > 0) {
            Console.WriteLine("    Strings:");
            foreach (var s in r.Strings) Console.WriteLine($"      - {s}");
          }
        }

        // If useful, try decoding the table using the meta version and show SystemEventLogs count
        if (File.Exists(metaPath)) {
          var version = File.ReadAllText(metaPath).Trim().Split('.');
          byte major = byte.Parse(version[0]);
          byte minor = byte.Parse(version[1]);
          var table = SmbiosTable.FromRawTableData(bytes[off..], major, minor);
          Console.WriteLine($"SmbiosTable.FromRawTableData -> SystemEventLogs: {table.SystemEventLogs.Count}");
        }
        // Stop after first successful parse for readability
        return;
      }
      catch {
        // ignore parse exceptions and try next offset
      }
    }

    Console.WriteLine("\nNo valid SMBIOS structure table found in the first 128 bytes of the file.");
    Console.WriteLine("If you produced this dump with dmidecode, ensure you used --dump-bin (not the text output).");
    Console.WriteLine("If file includes an entry-point header, try using the Windows reader (it strips the header), or run the offset scan above.");
  }

  private static string FormatHexWithSep(ulong value, int groupBytes) {
    // groupBytes: how many bytes per group (e.g., 8 -> groups of 8 bytes for 64-bit)
    // We'll format as hex with underscores every group of bytes for readability.
    var hex = value.ToString("X");
    if (hex.Length % 2 == 1) hex = "0" + hex;
    var bytes = Enumerable.Range(0, hex.Length / 2)
                         .Select(i => hex.Substring(i * 2, 2))
                         .ToArray();
    var groups = new System.Collections.Generic.List<string>();
    for (int i = 0; i < bytes.Length; i += groupBytes)
      groups.Add(string.Join(string.Empty, bytes.Skip(i).Take(groupBytes)));
    return "0x" + string.Join("_", groups);
  }

  // Add these methods near the bottom of Program class (or as static helpers).

  private static void DumpUnknownTypes(SmbiosTable table) {
    Console.WriteLine("\n=== Unknown / OEM Types (>=128) ===");
    var unknowns = table.RawStructures.Where(r => (byte)r.Type >= 128).ToList();
    if (unknowns.Count == 0) {
      Console.WriteLine("  (none)");
      return;
    }
    foreach (var raw in unknowns) {
      Console.WriteLine($"Type {(byte)raw.Type} Handle 0x{raw.Handle:X4} Len {raw.Length}");
      Console.WriteLine($"Formatted: {BitConverter.ToString(raw.FormattedArea.ToArray())}");
      if (raw.Strings.Count > 0) {
        Console.WriteLine("Strings:");
        foreach (var s in raw.Strings) Console.WriteLine($"  - {s}");
      }
      Console.WriteLine();
    }
  }

  private static void tryit() {
    var table = SmbiosTable.Load();
    // Example initialization inside your main table class
    var TemperatureProbes = new List<T028_TemperatureProbeInformation>();

    foreach (var raw in table.RawStructures) {
      // Ensure you are casting or comparing the enum integer cleanly
      if ((int)raw.Type == 28) {
        var parsedProbe = T028_TemperatureProbeInformation.Decode(raw);
        TemperatureProbes.Add(parsedProbe);
      }
    }
  }

  private static void RunSyntheticType15Test() {
    var bytes = new List<byte>();
    ushort handle15 = 0x1000;
    ushort logAreaLength = 0x0010;
    ushort headerStart = 0x0000;
    byte headerFormat = 1;
    byte headerLen = 4;
    byte accessMethod = 1;

    bytes.Add(15); // type
    bytes.Add(11); // length
    bytes.Add((byte)(handle15 & 0xFF));
    bytes.Add((byte)(handle15 >> 8));
    bytes.Add((byte)(logAreaLength & 0xFF));
    bytes.Add((byte)(logAreaLength >> 8));
    bytes.Add((byte)(headerStart & 0xFF));
    bytes.Add((byte)(headerStart >> 8));
    bytes.Add(headerFormat);
    bytes.Add(headerLen);
    bytes.Add(accessMethod);
    bytes.Add(0x00);
    bytes.Add(0x00);
    bytes.Add(127); bytes.Add(4); bytes.Add(0xFF); bytes.Add(0xFF); bytes.Add(0x00); bytes.Add(0x00);

    var table = SmbiosTable.FromRawTableData(bytes.ToArray(), 3, 4);
    Console.WriteLine($"\nSynthetic table: Raw count = {table.RawStructures.Count}");
    Console.WriteLine($"SystemEventLogs count = {table.SystemEventLogs.Count}");
    foreach (var log in table.SystemEventLogs) {
      Console.WriteLine($"  Handle: 0x{log.Handle:X4}");
      Console.WriteLine($"  LogAreaLength: {log.LogAreaLength}");
      Console.WriteLine($"  LogHeaderStartOffset: {log.LogHeaderStartOffset}");
      Console.WriteLine($"  LogHeaderFormat: {log.LogHeaderFormat}");
      Console.WriteLine($"  LogDataStartOffset: {log.LogDataStartOffset}");
      Console.WriteLine($"  AccessMethod: {log.AccessMethod}");
      Console.WriteLine($"  FormattedArea bytes: {BitConverter.ToString(log.FormattedArea.ToArray())}");
    }
  }
}