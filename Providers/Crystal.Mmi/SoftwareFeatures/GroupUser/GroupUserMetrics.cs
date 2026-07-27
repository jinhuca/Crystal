namespace Crystal.Mmi.SoftwareFeatures.GroupUser;

// Win32_GroupUser is a WMI association class (CIM_Component) — it has no scalar telemetry
// of its own. It relates a Win32_Group (GroupComponent) to the Win32_Account instance
// (a Win32_UserAccount or a nested Win32_Group) that is a member of that group. Both
// reference properties come back from WMI as embedded object-path strings, e.g.:
//   GroupComponent: Win32_Group.Domain="DESKTOP-01",Name="Administrators"
//   PartComponent:  Win32_UserAccount.Domain="DESKTOP-01",Name="jdoe"
public record GroupUserMetrics(
  string? GroupComponent,  // Win32_Group REF — the group
  string? PartComponent    // Win32_Account REF — the member (user or nested group)
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  public string? GroupName => ExtractKey(GroupComponent);
  public string? MemberName => ExtractKey(PartComponent);

  private static string? ExtractKey(string? path) =>
    string.IsNullOrEmpty(path) ? null : path.Split("Name=\"").LastOrDefault()?.TrimEnd('"');
}
