# Crystal

Crystal is a library for building composite .NET applications and components, with some battle-tested design principles and practices. It helps developers for architecting loosely coupled, testable, and maintainable applications with complex business logic and visual sophistication. It aims to provide ```complete``` infrastructure and glue for composite applications in various industries. It has been coming from ```collective``` knowledge and experience of many people in software industry and others. It is the result of software development practices in small companies and large corporations. Crystal has been building based upon the original <a href="https://www.microsoft.com/en-us/download/details.aspx?id=22379">```Microsoft Composite Application Guidance for WPF```</a> in 2008, and the Prism in 2009 under Microsoft Patterns and Practices, and now in Github <a href="https://github.com/prism">```Prism```</a>.

The Crystal have been created for modern applications targeting .NET 5.0 and above. It contains an integrated Dependency Injector, Module Management, and other features to provide ```complete``` toolkits for developers.
<br/>

#### Time line for Building Composite Applications: ####
<br/>

![Crystal Roadmap](https://github.com/jinhuca/Crystal/blob/master/Documentation/Crystal%20TimeLine.svg)
<br/>

#### Architectural Goals
`Composability`: Full support modularity at various level in application.
`Extensibility`: The ability to enhance, extend, or replace pieces of the Crystal Library without requiring users to redesign the applications.
`Performance`: The Crystal Library minimizes overhead while the application is running.
`Testability`: UI and business logic are to be tested.

#### Packages ####
Crystal library is built into two packages for two kinds of modules in .NET: `Crystal.Core` is designed for modules which refer to `Microsoft.NETCore.App`, and `Crystal.Desktop` is designed for modules which refer to `Microsoft.WindowsDesktop.App.WPF`:</br>

| <a href="https://www.nuget.org/packages/Crystal.Core/">![](https://img.shields.io/badge/Crystal-Core-orange)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Core)](https://www.nuget.org/packages/Crystal.Core/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) | 
|:----- |:----- |:----- |
| <a href="https://www.nuget.org/packages/Crystal.Desktop/">![](https://img.shields.io/badge/Crystal-Desktop-blue)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Desktop)](https://www.nuget.org/packages/Crystal.Desktop/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |

In this library, two packages are built to facilitate the UI customization:

| <a href="https://www.nuget.org/packages/Crystal.Behaviors/">![](https://img.shields.io/badge/Crystal-Behaviors-brightgreen)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Behaviors)](https://www.nuget.org/packages/Crystal.Behaviors/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |
|:----- |:----- |:----- |
| <a href="https://www.nuget.org/packages/Crystal.Themes/">![](https://img.shields.io/badge/Crystal-Themes-red)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Themes)](https://www.nuget.org/packages/Crystal.Themes/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |

#### [`Wiki`](https://github.com/jinhuca/Crystal/wiki) contains more details about key design and implementation about Crystal library.
#### [`Samples`](https://github.com/jinhuca/Crystal/tree/master/Samples) contains sample applications using Crystal library.
