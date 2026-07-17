using Crystal.Plot2D.Properties;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following set of attributes. 
// Change these attribute values to modify the information associated with an assembly.
[assembly: ComVisible(visibility: false)]
[assembly: CLSCompliant(isCompliant: true)]
[assembly: ThemeInfo(themeDictionaryLocation: ResourceDictionaryLocation.None, genericDictionaryLocation: ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Charts")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.DataSources.OneDimensional")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.DataSources.MultiDimensional")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Common")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Axes")]
//[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Isolines")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Graphs")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Filters")]
//[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Descriptions")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Converters")]
//[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.LegendItems")]
//[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Navigation")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Shapes")]

[assembly: XmlnsPrefix(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, prefix: "Plotter2D")]
[assembly: AllowPartiallyTrustedCallers]

namespace Crystal.Plot2D.Properties;

file static class AssemblyConstants {
  internal const string DefaultXmlNamespace = "http://schemas.crystal.com/Plot2D";
}
