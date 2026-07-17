using System.Linq;
using Crystal.Smbios.Types;
using Crystal.Smbios.Structures;
using Xunit;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class PortConnectorInformationTests
{
    private static byte[] MakePayload(
        PortConnectorType internalType = PortConnectorType.None,
        PortConnectorType externalType = PortConnectorType.UsbTypeCReceptacle,
        PortType portType = PortType.Usb)
    {
        var payload = new byte[0x09 - 4];
        payload[0x00] = 1; // InternalReferenceDesignator string
        payload[0x01] = (byte)internalType;
        payload[0x02] = 2; // ExternalReferenceDesignator string
        payload[0x03] = (byte)externalType;
        payload[0x04] = (byte)portType;
        return payload;
    }

    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = MakePayload(
            internalType: PortConnectorType.None,
            externalType: PortConnectorType.UsbTypeCReceptacle,
            portType: PortType.Usb);
        var table  = MakeTable(MakeStructure(8, 0x0070, payload, new[] { "J_USB1", "USB-C Port 1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var port = smbios.PortConnectors.FirstOrDefault();
        Assert.NotNull(port);
        Assert.Equal("J_USB1", port!.InternalReferenceDesignator);
        Assert.Equal(PortConnectorType.None, port.InternalConnectorType);
        Assert.Equal("USB-C Port 1", port.ExternalReferenceDesignator);
        Assert.Equal(PortConnectorType.UsbTypeCReceptacle, port.ExternalConnectorType);
        Assert.Equal(PortType.Usb, port.PortType);
    }

    [Fact]
    public void Decode_Rj45NetworkPort_DecodedCorrectly()
    {
        var payload = MakePayload(
            internalType: PortConnectorType.OnBoardIde, // arbitrary internal value, unrelated
            externalType: PortConnectorType.Rj45,
            portType: PortType.NetworkPort);
        var table  = MakeTable(MakeStructure(8, 0x0071, payload, new[] { "J_LAN1", "Ethernet" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var port = smbios.PortConnectors.First();
        Assert.Equal(PortConnectorType.Rj45, port.ExternalConnectorType);
        Assert.Equal(PortType.NetworkPort, port.PortType);
    }

    [Fact]
    public void Decode_HeadphoneJack_DecodedCorrectly()
    {
        var payload = MakePayload(
            externalType: PortConnectorType.MiniJackHeadphones,
            portType: PortType.AudioPort);
        var table  = MakeTable(MakeStructure(8, 0x0072, payload, new[] { "J_AUD1", "Headphone/Mic" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var port = smbios.PortConnectors.First();
        Assert.Equal(PortConnectorType.MiniJackHeadphones, port.ExternalConnectorType);
        Assert.Equal(PortType.AudioPort, port.PortType);
    }

    [Fact]
    public void Decode_NoStrings_ReferenceDesignatorsAreNull()
    {
        // Some internal-only connectors (e.g. onboard IDE header) may have no
        // string labels at all.
        var payload = MakePayload();
        payload[0x00] = 0; // string index 0 = "not present"
        payload[0x02] = 0;
        var table  = MakeTable(MakeStructure(8, 0x0073, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var port = smbios.PortConnectors.First();
        Assert.Null(port.InternalReferenceDesignator);
        Assert.Null(port.ExternalReferenceDesignator);
    }

    [Fact]
    public void Decode_LegacyStructureWithoutPortType_DefaultsToNone()
    {
        // A structure truncated right after ExternalConnectorType (0x08 bytes total,
        // no PortType byte) — must not throw and must default sensibly.
        var payload = new byte[0x08 - 4];
        payload[0x00] = 1;
        payload[0x01] = (byte)PortConnectorType.Db9PinMale;
        payload[0x02] = 2;
        payload[0x03] = (byte)PortConnectorType.Db9PinFemale;

        var table  = MakeTable(MakeStructure(8, 0x0074, payload, new[] { "J_COM1", "Serial Port" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var port = smbios.PortConnectors.First();
        Assert.Equal(PortType.None, port.PortType);
        Assert.Equal(PortConnectorType.Db9PinFemale, port.ExternalConnectorType);
    }

    [Fact]
    public void MultipleConnectors_AllDecodedIndependently()
    {
        var usb = MakeStructure(8, 0x0075, MakePayload(externalType: PortConnectorType.UsbTypeCReceptacle, portType: PortType.Usb),
            new[] { "J_USB2", "USB-C Port 2" });
        var hdmi = MakeStructure(8, 0x0076, MakePayload(externalType: PortConnectorType.Other, portType: PortType.VideoPort),
            new[] { "J_HDMI1", "HDMI Out" });
        var lan = MakeStructure(8, 0x0077, MakePayload(externalType: PortConnectorType.Rj45, portType: PortType.NetworkPort),
            new[] { "J_LAN1", "Ethernet" });

        var table  = MakeTable(usb, hdmi, lan);
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Equal(3, smbios.PortConnectors.Count);
        Assert.Contains(smbios.PortConnectors, p => p.PortType == PortType.Usb);
        Assert.Contains(smbios.PortConnectors, p => p.PortType == PortType.VideoPort);
        Assert.Contains(smbios.PortConnectors, p => p.PortType == PortType.NetworkPort);
    }
}
