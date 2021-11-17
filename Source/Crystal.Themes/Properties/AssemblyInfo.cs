using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: SuppressMessage("Microsoft.Performance", "CA1824:MarkAssembliesWithNeutralResourcesLanguage")]
[assembly: System.Resources.NeutralResourcesLanguage("en", System.Resources.UltimateResourceFallbackLocation.MainAssembly)]

[assembly: XmlnsPrefix(@"http://schemas.crystal.com/themes", "crystal")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Behaviors")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Theming")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Automation.Peers")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Accessibility")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.ValueBoxes")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Actions")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Converters")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Markup")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Controls")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Controls.Dialogs")]
[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes", "Crystal.Themes.Windows.Shell")]

//[assembly: XmlnsPrefix(@"http://schemas.crystal.com/themes/shared", "crystalShared")]
//[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes/shared", "Crystal.Themes.Behaviors")]
//[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes/shared", "Crystal.Themes.Actions")]
//[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes/shared", "Crystal.Themes.Converters")]
//[assembly: XmlnsDefinition(@"http://schemas.crystal.com/themes/shared", "Crystal.Themes.Markup")]

[assembly: SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.")]
[assembly: SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0006:Name of CoerceValueCallback should match registered name.")]
[assembly: SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0007:Name of ValidateValueCallback should match registered name.")]
[assembly: SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0036:Avoid side effects in CLR accessors.")]
[assembly: SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0041:Set mutable dependency properties using SetCurrentValue.")]