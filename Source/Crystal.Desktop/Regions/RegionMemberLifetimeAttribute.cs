namespace Crystal;

/// <summary>
/// When <see cref="RegionMemberLifetimeAttribute"/> is applied to class provides data
/// the <see cref="RegionMemberLifetimeBehavior"/> can use to determine if the instance should
/// be removed when it is deactivated.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class RegionMemberLifetimeAttribute : Attribute
{
  ///<summary>
  /// Determines if the region member should be kept-alive
  /// when deactivated.
  ///</summary>
  public bool KeepAlive { get; set; } = true;
}