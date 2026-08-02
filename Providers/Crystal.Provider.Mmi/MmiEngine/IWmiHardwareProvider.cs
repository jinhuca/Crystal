using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

/// <summary>
/// Abstraction over the Microsoft.Management.Infrastructure (MMI) client surface used by
/// Crystal.Provider.Mmi. Implementations translate native CIM operations into the immutable
/// <see cref="WmiValue"/> representation consumed by the feature extension methods.
/// </summary>
public interface IWmiHardwareProvider {
  /// <summary>
  /// Enumerates all instances of the specified WMI class in the default
  /// <c>root\cimv2</c> namespace.
  /// </summary>
  /// <param name="wmiClassName">The WMI/CIM class name to query, e.g. <c>Win32_Battery</c>.</param>
  /// <param name="cancellationToken">Token used to cancel the streaming query.</param>
  /// <returns>The materialized instances, one property bag per instance.</returns>
  Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken);

  /// <summary>
  /// Enumerates all instances of the specified WMI class in an explicit namespace.
  /// Use this for classes that do not live under <c>root\cimv2</c>, such as
  /// <c>Win32_Tpm</c> (<c>root\cimv2\Security\MicrosoftTpm</c>).
  /// </summary>
  /// <param name="namespaceName">The CIM namespace, e.g. <c>root\cimv2\Security\MicrosoftTpm</c>.</param>
  /// <param name="wmiClassName">The WMI/CIM class name to query.</param>
  /// <param name="cancellationToken">Token used to cancel the streaming query.</param>
  /// <returns>The materialized instances, one property bag per instance.</returns>
  Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName,
      string wmiClassName,
      CancellationToken cancellationToken);

  /// <summary>
  /// Enumerates instances produced by an arbitrary WQL query (for example an
  /// <c>ASSOCIATORS OF</c> or <c>REFERENCES OF</c> traversal, or a filtered
  /// <c>SELECT ... WHERE ...</c>).
  /// </summary>
  /// <param name="namespaceName">The CIM namespace to run the query in.</param>
  /// <param name="wqlQuery">A complete WQL query string.</param>
  /// <param name="cancellationToken">Token used to cancel the streaming query.</param>
  /// <returns>The materialized instances, one property bag per instance.</returns>
  Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
      string namespaceName,
      string wqlQuery,
      CancellationToken cancellationToken);

  /// <summary>
  /// Invokes a static method on a WMI class (for example <c>Win32_Process.Create</c>).
  /// </summary>
  /// <param name="namespaceName">The CIM namespace containing the class.</param>
  /// <param name="wmiClassName">The class exposing the static method.</param>
  /// <param name="methodName">The method to invoke.</param>
  /// <param name="inParameters">Input parameters keyed by name; may be empty.</param>
  /// <param name="cancellationToken">Token used to cancel the invocation.</param>
  /// <returns>The method's return value and output parameters, or <see cref="WmiMethodResult.Empty"/> on failure.</returns>
  Task<WmiMethodResult> InvokeStaticMethodAsync(
      string namespaceName,
      string wmiClassName,
      string methodName,
      IReadOnlyDictionary<string, WmiValue> inParameters,
      CancellationToken cancellationToken);
}
