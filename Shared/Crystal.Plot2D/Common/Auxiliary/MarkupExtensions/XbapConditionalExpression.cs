using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common.Auxiliary.MarkupExtensions;

[EditorBrowsable(state: EditorBrowsableState.Never)]
public sealed class XbapConditionalExpression : MarkupExtension {
  public XbapConditionalExpression() { }

  public XbapConditionalExpression(object value) {
    Value = value;
  }

  [ConstructorArgument(argumentName: "value")]
  public object Value { get; }

  public override object ProvideValue(IServiceProvider serviceProvider) {
#if RELEASEXBAP
			return null;
#else
    return ((ResourceDictionary)Application.LoadComponent(resourceLocator: new Uri(uriString: Constants.Constants.ThemeUri, uriKind: UriKind.Relative)))[key: Value];
#endif
  }
}
