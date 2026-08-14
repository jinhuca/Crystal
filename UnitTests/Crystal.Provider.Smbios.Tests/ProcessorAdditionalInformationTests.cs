using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class ProcessorAdditionalInformationTests
{
    [Fact]
    public void Decode_PopulatesReferencedHandleAndSpecificData()
    {
        var payload = new byte[]
        {
            0x04, 0x00, // ReferencedHandle = 0x0004
            0x05,       // ProcessorSpecificBlockLength = 5 (2-byte header + 3 data bytes)
            0x07,       // ProcessorArchitectureType = RiscVRv64
            0x11, 0x22, 0x33, // ProcessorSpecificData
        };

        var table = MakeTable(MakeStructure(44, 0x0240, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var p = smbios.ProcessorAdditionalInformation[0];
        Assert.Equal((ushort)0x0004, p.ReferencedHandle);
        Assert.Equal((byte)5, p.ProcessorSpecificBlockLength);
        Assert.Equal(ProcessorSpecificBlockArchitectureType.RiscVRv64, p.ProcessorArchitectureType);
        Assert.Equal(3, p.ProcessorSpecificData.Count);
        Assert.Equal((byte)0x11, p.ProcessorSpecificData[0]);
        Assert.Equal((byte)0x33, p.ProcessorSpecificData[2]);
    }

    [Fact]
    public void Decode_EmptySpecificBlock_NoData()
    {
        var payload = new byte[] { 0x04, 0x00, 0x02, 0x00 }; // block length 2 = header only, arch Reserved
        var table = MakeTable(MakeStructure(44, 0x0241, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var p = smbios.ProcessorAdditionalInformation[0];
        Assert.Empty(p.ProcessorSpecificData);
    }
}
