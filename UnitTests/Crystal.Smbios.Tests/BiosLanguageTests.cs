using Crystal.Smbios.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Crystal.Smbios.Tests;
public class BiosLanguageTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[0x07 - 4]; // offsets 0x04..0x06
        payload[0x00] = 2; // two installable languages
        payload[0x01] = 0x01; // flags (arbitrary)
        payload[0x02] = 1; // current language = string #1

        var table  = MakeTable(MakeStructure(13, 0x0090, payload, new[] { "en-US", "fr-FR" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var b = smbios.BiosLanguages.FirstOrDefault();
        Assert.NotNull(b);
        Assert.Equal(2, b!.InstallableLanguages);
        Assert.Equal(0x01, b.Flags);
        Assert.Equal("en-US", b.CurrentLanguage);
    }

    [Fact]
    public void Decode_ShortStructure_UsesDefaults()
    {
        var payload = new byte[0x05 - 4]; // only installable languages present
        payload[0x00] = 1;

        var table  = MakeTable(MakeStructure(13, 0x0091, payload, new[] { "en-US" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var b = smbios.BiosLanguages.First();
        Assert.Equal(1, b.InstallableLanguages);
        Assert.Equal((byte)0, b.Flags);
        Assert.Null(b.CurrentLanguage);
    }

    private static byte[] MakeStructure(int type, ushort handle, byte[] payload, string[] strings)
    {
        // SMBIOS structure header: Type (1), Length (1), Handle (2 little-endian)
        var header = new byte[4];
        header[0] = (byte)type;
        header[1] = (byte)(4 + (payload?.Length ?? 0));
        header[2] = (byte)(handle & 0xFF);
        header[3] = (byte)((handle >> 8) & 0xFF);

        var bytes = new List<byte>();
        bytes.AddRange(header);
        if (payload != null && payload.Length > 0) bytes.AddRange(payload);

        // add string-set: each string null-terminated, then a double-null terminator
        if (strings != null)
        {
            foreach (var s in strings)
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(s));
                bytes.Add(0);
            }
        }
        bytes.Add(0); // end of structure string-set

        return bytes.ToArray();
    }

    private static byte[] MakeTable(params byte[][] structures)
    {
        var all = new List<byte>();
        foreach (var s in structures) if (s != null) all.AddRange(s);
        return all.ToArray();
    }
}
