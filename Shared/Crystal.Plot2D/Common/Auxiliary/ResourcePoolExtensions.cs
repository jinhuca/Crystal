namespace Crystal.Plot2D.Common.Auxiliary;

internal static class ResourcePoolExtensions {
  public static T GetOrCreate<T>(this ResourcePool<T> pool) where T : new() {
    var instance = pool.Get() ?? new T();

    return instance;
  }
}
