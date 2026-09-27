using System.Runtime.Serialization;

namespace Crystal.Infrastructure.Core.Navigation.Regions;

/// <summary>
/// An exception when there is an issue with a View being added to a Region
/// </summary>
#if NETFRAMEWORK
[Serializable]
#endif
public sealed class RegionViewException : RegionException {
  /// <summary>
  /// Initializes a new <see cref="RegionViewException"/>
  /// </summary>
  public RegionViewException() {
  }

  /// <summary>
  /// Initializes a new <see cref="RegionViewException"/>
  /// </summary>
  /// <param name="message">The Exception Message.</param>
  public RegionViewException(string message) : base(message) {
  }

#if NETFRAMEWORK
    /// <inheritdoc />
    public RegionViewException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif

  /// <summary>
  /// Initializes a new <see cref="RegionViewException"/>
  /// </summary>
  /// <param name="message">The Exception Message.</param>
  /// <param name="innerException">The Inner <see cref="Exception"/>.</param>
  public RegionViewException(string message, Exception innerException) : base(message, innerException) {
  }
}
