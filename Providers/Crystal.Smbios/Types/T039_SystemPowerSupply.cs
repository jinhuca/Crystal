namespace Crystal.Smbios.Types;

/// <summary>
/// Type 39 — System Power Supply (DSP0134 §7.39)
/// </summary>
public sealed class T039_SystemPowerSupply : ISmbiosDecodedStructure
{
    public SmbiosStructureType StructureType { get; init; }
    public byte Length { get; init; }
    public ushort Handle { get; init; }
    public byte PowerUnitGroup { get; init; }
    public byte LocationAndStatus { get; init; }
    public byte PowerSupplyType { get; init; }
    public byte InputVoltageRangeSwitch { get; init; }
    public ushort CapacityWatts { get; init; }
    public byte DescriptionIndex { get; init; }

    internal static T039_SystemPowerSupply Decode(SmbiosRawStructure s)
    {
        return new T039_SystemPowerSupply
        {
            StructureType = s.Type,
            Length = s.Length,
            Handle = s.Handle,
            PowerUnitGroup = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
            LocationAndStatus = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
            PowerSupplyType = s.Length > 0x06 ? s.ReadByte(0x06) : (byte)0,
            InputVoltageRangeSwitch = s.Length > 0x07 ? s.ReadByte(0x07) : (byte)0,
            CapacityWatts = s.Length > 0x09 ? s.ReadWord(0x08) : (ushort)0,
            DescriptionIndex = s.Length > 0x0B ? s.ReadByte(0x0A) : (byte)0,
        };
    }
}
