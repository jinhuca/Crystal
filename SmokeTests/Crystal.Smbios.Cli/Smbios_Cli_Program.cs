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
          Console.WriteLine($"  ErrorType: {info.ErrorType}  Granularity: {info.ErrorGranularity}  VendorSyndrome: 0x{info.VendorSyndrome:X}");
          Console.WriteLine($"    MemoryArrayHandle: 0x{info.MemoryArrayHandle:X4}  DeviceHandle: 0x{info.DeviceHandle:X4}  Is64: {info.Is64Bit}");
          if (entry.Is64) {
            var e64 = entry.As64!;
            Console.WriteLine($"    ErrorOperation: 0x{e64.ErrorOperation:X4}");
            Console.WriteLine($"    PhysicalAddress: {FormatHexWithSep(e64.PhysicalAddress, 8)}  Mask: {FormatHexWithSep(e64.PhysicalAddressMask, 8)}");
          }
          else if (entry.Is32) {
            var e32 = entry.As32!;
            Console.WriteLine($"    PhysicalAddress: {FormatHexWithSep(e32.PhysicalAddress, 4)}  AddressResolution: {FormatHexWithSep(e32.AddressResolution, 4)}");
          }
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
          Console.WriteLine($"    Capabilities: {device.Capabilities}");
          Console.WriteLine($"    Accuracy: {(device.Accuracy > 0 ? $"{device.Accuracy / 10.0:F1}% (1/10 of a percentage point)" : "Not specified")}");
          Console.WriteLine($"    Track Speed: {(device.TrackSpeed > 0 ? device.TrackSpeed.ToString() : "Not specified")}");
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
          Console.WriteLine($"    InterleavePosition: {(m.InterleavePosition.HasValue ? m.InterleavePosition.Value.ToString() : "(none)")}  InterleaveGranularityBytes: {(m.InterleaveGranularityBytes.HasValue ? m.InterleaveGranularityBytes.Value.ToString() : "(none)")}");
          if (m.InterleaveGranularityBytes is not null && m.PartitionWidth > 1 && m.InterleavePosition is not null) {
            var ip = m.InterleavePosition.Value;
            if (ip >= 1 && ip <= m.PartitionWidth) {
              Console.WriteLine("    Segments for this partition:");
              foreach (var seg in MemoryInterleaveHelper.ComputeInterleavedSegments(m, (int)ip))
                Console.WriteLine($"      0x{seg.Offset:X16} length={seg.Length}");
            }
            else {
              Console.WriteLine($"    Warning: invalid InterleavePosition={ip} (expected 1..{m.PartitionWidth}) — cannot compute segments.");
            }
          }
        }
      }

      Console.WriteLine("\n=== Memory Device Mapped Addresses (Type 20) ===");
      if (table.MemoryDeviceMappedAddresses.Count == 0)
        Console.WriteLine("  (none reported)");
      else {
        foreach (var md in table.MemoryDeviceMappedAddresses) {
          Console.WriteLine($"  DeviceHandle: 0x{md.MemoryDeviceHandle:X4}  Start: 0x{md.StartAddressBytes:X}  End: 0x{md.EndAddressBytes:X}");
          Console.WriteLine($"    StartKiB: {md.StartAddressKiB}  EndKiB: {md.EndAddressKiB}  SizeKiB: {md.SizeKiB}");
          Console.WriteLine($"    UsesExtended: {md.UsesExtendedAddresses}  PartitionRowPosition: {md.PartitionRowPosition}");
          Console.WriteLine($"    InterleavePosition: {(md.InterleavePosition.HasValue ? md.InterleavePosition.Value.ToString() : "(none)")}  InterleaveColumn: {(md.InterleaveColumn.HasValue ? md.InterleaveColumn.Value.ToString() : "(none)")}  GranularityBytes: {(md.InterleaveGranularityBytes.HasValue ? md.InterleaveGranularityBytes.Value.ToString() : "(none)")}");
          if (md.InterleaveGranularityBytes is not null && md.PartitionRowPosition > 1 && md.InterleavePosition is not null) {
            var ip = md.InterleavePosition.Value;
            if (ip >= 1 && ip <= md.PartitionRowPosition) {
              Console.WriteLine("    Segments for this partition-row:");
              foreach (var seg in MemoryInterleaveHelper.ComputeInterleavedSegments(md, (int)ip))
                Console.WriteLine($"      0x{seg.Offset:X16} length={seg.Length}");
            }
            else {
              Console.WriteLine($"    Warning: invalid InterleavePosition={ip} (expected 1..{md.PartitionRowPosition}) — cannot compute segments.");
            }
          }
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
        var psuRawList = table.RawStructures.Where(s => s.Type == (SmbiosStructureType)39).ToList();
        foreach (var pair in table.PowerSupplies.Zip(psuRawList, (p, r) => (Psu: p, Raw: r))) {
          var psu = pair.Psu;
          var raw = pair.Raw;
          var description = raw.GetString(psu.DescriptionIndex) ?? "(unnamed)";
          Console.WriteLine($"  Description: {description}");
          Console.WriteLine($"    Power Unit Group: {psu.PowerUnitGroup}");
          Console.WriteLine($"    Location and Status: 0x{psu.LocationAndStatus:X2}");
          Console.WriteLine($"    Power Supply Type: 0x{psu.PowerSupplyType:X2}");
          Console.WriteLine($"    Input Voltage Range Switch: 0x{psu.InputVoltageRangeSwitch:X2}");
          Console.WriteLine($"    Capacity: {psu.CapacityWatts} W");
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
    }
    catch (Exception ex) {
      Console.WriteLine($"Error: {ex.Message}");
      Console.WriteLine(ex.StackTrace);
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
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
      Console.WriteLine($"  LogHeaderLength: {log.LogHeaderLength}");
      Console.WriteLine($"  AccessMethod: {log.AccessMethod}");
      Console.WriteLine($"  FormattedArea bytes: {BitConverter.ToString(log.FormattedArea.ToArray())}");
    }
  }
}