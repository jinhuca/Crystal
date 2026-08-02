using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

/// <summary>
/// Convenience helpers that build WQL <c>ASSOCIATORS OF</c> / <c>REFERENCES OF</c>
/// traversals over <see cref="IWmiHardwareProvider.QueryAsync"/>.
/// </summary>
public static class WmiAssociationExtensions {
  private const string DefaultNamespace = @"root\cimv2";

  /// <summary>
  /// Returns the instances associated with the object identified by
  /// <paramref name="objectPath"/> (a WMI object path such as
  /// <c>Win32_NetworkAdapter.DeviceID="1"</c>).
  /// </summary>
  /// <param name="provider">The MMI provider.</param>
  /// <param name="objectPath">The source object path to traverse from.</param>
  /// <param name="cancellationToken">Token used to cancel the query.</param>
  /// <param name="resultClass">Optional class filter for the associated endpoints.</param>
  /// <param name="assocClass">Optional association-class filter.</param>
  /// <param name="namespaceName">The CIM namespace; defaults to <c>root\cimv2</c>.</param>
  public static Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetAssociatorsAsync(
      this IWmiHardwareProvider provider,
      string objectPath,
      CancellationToken cancellationToken,
      string? resultClass = null,
      string? assocClass = null,
      string namespaceName = DefaultNamespace) {
    var query = $"ASSOCIATORS OF {{{objectPath}}}";
    var filters = new List<string>(2);
    if (!string.IsNullOrWhiteSpace(resultClass)) filters.Add($"ResultClass = {resultClass}");
    if (!string.IsNullOrWhiteSpace(assocClass)) filters.Add($"AssocClass = {assocClass}");
    if (filters.Count > 0) query += " WHERE " + string.Join(" ", filters);

    return provider.QueryAsync(namespaceName, query, cancellationToken);
  }

  /// <summary>
  /// Returns the association instances that reference the object identified by
  /// <paramref name="objectPath"/>.
  /// </summary>
  /// <param name="provider">The MMI provider.</param>
  /// <param name="objectPath">The source object path to traverse from.</param>
  /// <param name="cancellationToken">Token used to cancel the query.</param>
  /// <param name="resultClass">Optional association-class filter.</param>
  /// <param name="namespaceName">The CIM namespace; defaults to <c>root\cimv2</c>.</param>
  public static Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetReferencesAsync(
      this IWmiHardwareProvider provider,
      string objectPath,
      CancellationToken cancellationToken,
      string? resultClass = null,
      string namespaceName = DefaultNamespace) {
    var query = $"REFERENCES OF {{{objectPath}}}";
    if (!string.IsNullOrWhiteSpace(resultClass)) query += $" WHERE ResultClass = {resultClass}";

    return provider.QueryAsync(namespaceName, query, cancellationToken);
  }
}
