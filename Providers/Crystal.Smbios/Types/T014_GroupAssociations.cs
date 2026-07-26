using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// A single member referenced by a Type 14 Group Associations structure.
/// </summary>
public readonly record struct GroupAssociationItem(SmbiosStructureType ItemType, ushort ItemHandle);

/// <summary>
/// Type 14 — Group Associations (DSP0134 §7.15).
/// Lets OEMs describe an arbitrary hierarchy/grouping of other structures
/// (including other Group Associations structures, allowing nested groups).
/// </summary>
public sealed class T014_GroupAssociations : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? GroupName { get; init; }

  /// <summary>Member structures in this group; count derived from the structure length.</summary>
  public IReadOnlyList<GroupAssociationItem> Items { get; init; } = Array.Empty<GroupAssociationItem>();

  internal static T014_GroupAssociations Decode(SmbiosRawStructure s) {
    var items = new List<GroupAssociationItem>();
    int count = (s.Length - 0x05) / 3;
    for (int i = 0; i < count; i++) {
      int offset = 0x05 + i * 3;
      if (s.Length < offset + 3) break;
      items.Add(new GroupAssociationItem(
          (SmbiosStructureType)s.ReadByte(offset),
          s.ReadWord(offset + 1)));
    }

    return new T014_GroupAssociations {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      GroupName = s.GetString(s.ReadByte(0x04)),
      Items = items,
    };
  }
}
