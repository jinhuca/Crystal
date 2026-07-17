using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Markup;
using JinHu.Visualization.Plotter2D;

[module: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "JinHu.Visualization.Plotter2D")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "JinHu.Visualization.Plotter2D.Charts")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "JinHu.Visualization.Plotter2D.DataSources")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "JinHu.Visualization.Plotter2D.Common")]
[assembly: XmlnsPrefix(AssemblyConstants.DefaultXmlNamespace, "Plotter2D")]

[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]

namespace JinHu.Visualization.Plotter2D
{
	public static class AssemblyConstants
	{
    public const string DefaultXmlNamespace = "http://jinhuca.visualization.com/Plotter2D/1.0";
	}
}
