using System;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common.Auxiliary.MarkupExtensions;

/// <summary>
///   Represents a markup extension, which allows to get an access to application resource files.
/// </summary>
[MarkupExtensionReturnType(returnType: typeof(string))]
public sealed class ResourceExtension : MarkupExtension {
  /// <summary>
  ///   Initializes a new instance of the <see cref="ResourceExtension"/> class.
  /// </summary>
  public ResourceExtension() { }

  private string resourceKey;
  //[ConstructorArgument("resourceKey")]
  public string ResourceKey {
    get => resourceKey;
    set {
      if(resourceKey == null) {
        throw new ArgumentNullException(paramName: "resourceKey");
      }

      resourceKey = value;
    }
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ResourceExtension"/> class.
  /// </summary>
  /// <param name="_resourceKey">
  ///   The resource key.
  /// </param>
  public ResourceExtension(string _resourceKey) {
    resourceKey = _resourceKey ?? throw new ArgumentNullException(paramName: "resourceKey");
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
    => Strings.UIResources.ResourceManager.GetString(name: resourceKey);
}
