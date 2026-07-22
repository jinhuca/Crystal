using System.Collections.Frozen;
using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.Tests.Helpers;

internal static class DemoProviders
{
    public static FakeWmiProvider CacheMemory()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("L3 Cache")),
            ("Caption", new WmiValue("L3 Cache")),
            ("InstalledSize", new WmiValue(32768)),
            ("MaxCacheSize", new WmiValue(32768)),
            ("CacheType", new WmiValue(5)),
            ("Level", new WmiValue(5)),
            ("Associativity", new WmiValue(7)),
            ("LineSize", new WmiValue(64)),
            ("Status", new WmiValue("OK")));

        return new FakeWmiProvider("Win32_CacheMemory", new[] { row });
    }

    public static FakeWmiProvider PhysicalMemoryArray()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("Physical Memory Array")),
            ("Manufacturer", new WmiValue("Dell Inc.")),
            ("Location", new WmiValue(3)),
            ("Use", new WmiValue(3)),
            ("MemoryDevices", new WmiValue(4)),
            ("MemoryErrorCorrection", new WmiValue(6)),
            ("MaxCapacityEx", new WmiValue(137438953472UL)),
            ("Status", new WmiValue("OK")));

        return new FakeWmiProvider("Win32_PhysicalMemoryArray", new[] { row });
    }

    public static FakeWmiProvider SystemSlot()
    {
        var row = WmiRow.Build(
            ("SlotDesignation", new WmiValue("PCIEX16_1")),
            ("Name", new WmiValue("PCI Express x16 Slot")),
            ("CurrentUsage", new WmiValue(4)),
            ("MaxDataWidth", new WmiValue(6)),
            ("Manufacturer", new WmiValue("ASUSTeK")),
            ("PMESignal", new WmiValue(true)),
            ("SupportsHotPlug", new WmiValue(false)),
            ("Status", new WmiValue("OK")));

        return new FakeWmiProvider("Win32_SystemSlot", new[] { row });
    }

    public static FakeWmiProvider MemoryTopology()
    {
        FrozenDictionary<string, WmiValue> array = WmiRow.Build(
            ("Name", new WmiValue("Physical Memory Array")),
            ("MemoryDevices", new WmiValue(2)),
            ("MaxCapacityEx", new WmiValue(68719476736UL)));

        FrozenDictionary<string, WmiValue> dimm0 = WmiRow.Build(
            ("BankLabel", new WmiValue("BANK 0")),
            ("Capacity", new WmiValue(17179869184UL)));

        FrozenDictionary<string, WmiValue> dimm1 = WmiRow.Build(
            ("BankLabel", new WmiValue("BANK 1")),
            ("Capacity", new WmiValue(17179869184UL)));

        FrozenDictionary<string, WmiValue> cache = WmiRow.Build(
            ("Name", new WmiValue("L3 Cache")),
            ("InstalledSize", new WmiValue(32768)),
            ("Level", new WmiValue(5)),
            ("CacheType", new WmiValue(5)));

        return new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_PhysicalMemoryArray"] = new[] { array },
            ["Win32_PhysicalMemory"] = new[] { dimm0, dimm1 },
            ["Win32_CacheMemory"] = new[] { cache }
        });
    }

    public static FakeWmiProvider Empty(string className)
    {
        return new FakeWmiProvider(className, WmiRow.Empty());
    }
}
