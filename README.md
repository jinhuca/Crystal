# Crystal

```Crystal``` is a library for building composite .NET applications and components, with some battle-tested design principles and practices. It helps developers for architecting loosely coupled, testable, and maintainable applications with complex business logic and visual sophistication.

* It aims to provide ```complete``` infrastructure and glue for composite applications in various industries.

* It has been coming from ```collective``` knowledge and experience of many people in software industry and others.

* It is the result of software development practices in small companies and large corporations, as well as on streets.

* ```Crystal.Core``` has been building based upon the original <a href="https://www.microsoft.com/en-us/download/details.aspx?id=22379">```Microsoft Composite Application Guidance for WPF```</a> in 2008, and the <a href="https://prismlibrary.com/">```Prism```</a> in 2009 under Microsoft Patterns and Practices, and now in Github <a href="https://github.com/prism">```Prism```</a>.

The ```Crystal.Core``` have been created significant different from Prism, and targeted the applications for modern .NET 5.0 and above. It contains a default Dependency Injector, Module Management, and other features to provide ```complete``` toolkits for developers.
<br/>

| Packages | Nuget | License |
| :--- | :--- | :---|
|<a href="https://www.nuget.org/packages/Crystal.Desktop/">![](https://img.shields.io/badge/Crystal-Desktop-blue)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Desktop)](https://www.nuget.org/packages/Crystal.Desktop/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |
<a href="https://www.nuget.org/packages/Crystal.Infrastructure/">![](https://img.shields.io/badge/Crystal-Infrastructure-brightgreen)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Infrastructure)](https://www.nuget.org/packages/Crystal.Infrastructure/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |

#### Time line for Building Composite Applications: ####
<br/>

![Crystal Roadmap](https://github.com/jinhuca/Crystal/blob/master/Documentation/Crystal%20TimeLine.svg)
<br/>

#### Introduction ####
We overview some major components built in <a href="https://github.com/jinhuca/Crystal.Infrastructure">```Crystal```</a>. Visit <a href="https://github.com/jinhuca/Crystal.Infrastructure/wiki">```wiki```</a> for more detailed information and tutorials.

```Challenges```<br/>
In the whole life cycles of real-world applications, developers face a number of challenges:
* Application requirements change over time.
* Maintain the complex codebase.
* Add/remove features/components.
* Deploy application in regular basis.
It requires an architecture that allows individual parts of the application to be independently developed and tested, and then modified or updated later, in isolation, without affecting the rest of the application.

```Architectural Goals```<br/>
* Composability: Full support modularity at various level in application.
* Extensibility: The ability to enhance, extend, or replace pieces of the Crystal Library without requiring users to redesign the applications.
* Performance: The Crystal Library minimizes overhead while the application is running.
* Testability: UI and business logic are to be tested.

[```Solid Principles```](https://github.com/jinhuca/Crystal/wiki/02.-Solid-Principles)
[```Composite Approach```](https://github.com/jinhuca/Crystal/wiki/03.--Composite-Approach)
[```Key Concepts```](https://github.com/jinhuca/Crystal/wiki/04.-Key-Concepts)
[```Application and Shell```](https://github.com/jinhuca/Crystal/wiki/05.---Application-and-Shell)
[```Bootstrappers```](https://github.com/jinhuca/Crystal/wiki/06.-Bootstrappers)
[```Regions```](https://github.com/jinhuca/Crystal/wiki/07.-Regions)
[```Views```](https://github.com/jinhuca/Crystal/wiki/08.-Views)
[```Modules```](https://github.com/jinhuca/Crystal/wiki/09.-Modules)
[```Commands```](https://github.com/jinhuca/Crystal/wiki/10.-Commands)
[```MVVM```](https://github.com/jinhuca/Crystal/wiki/11.-MVVM)
[```Event Aggregators```](https://github.com/jinhuca/Crystal/wiki/12.-Event-Aggregators)<br/>
