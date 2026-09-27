using System.Runtime.Serialization;

namespace Crystal.Infrastructure.Core.Navigation.Regions;

/// <summary>
/// Provides a common base class for Region Exceptions
/// </summary>
public abstract class RegionException : Exception {
  /// <inheritdoc />
  protected RegionException() {
  }

  /// <inheritdoc />
  protected RegionException(string message) : base(message) {
  }

#if NETFRAMEWORK
    /// <inheritdoc />
    protected RegionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif

  /// <inheritdoc />
  protected RegionException(string message, Exception innerException) : base(message, innerException) {
  }
}
