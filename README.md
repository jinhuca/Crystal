# Crystal

```Crystal``` is a library for building composite .NET applications and components, with some battle-tested design principles and practices. It helps developers for architecting loosely coupled, testable, and maintainable applications with complex business logic and visual sophistication.

* It aims to provide ```complete``` infrastructure and glue for composite applications in various industries.

* It has been coming from ```collective``` knowledge and experience of many people in software industry and others.

* It is the result of software development practices in small companies and large corporations, as well as on streets.

* ```Crystal.Core``` has been building based upon the original <a href="https://www.microsoft.com/en-us/download/details.aspx?id=22379">```Microsoft Composite Application Guidance for WPF```</a> in 2008, and the Prism in 2009 under Microsoft Patterns and Practices, and now in Github <a href="https://github.com/prism">```Prism```</a>.

The ```Crystal.Core``` have been created significant different from Prism, and targeted the applications for modern .NET 5.0 and above. It contains a default Dependency Injector, Module Management, and other features to provide ```complete``` toolkits for developers.
<br/>

#### Time line for Building Composite Applications: ####
<br/>

![Crystal Roadmap](https://github.com/jinhuca/Crystal/blob/master/Documentation/Crystal%20TimeLine.svg)
<br/>

#### Introduction ####
We overview some major components built in <a href="https://github.com/jinhuca/Crystal">```Crystal```</a>. Visit <a href="https://github.com/jinhuca/Crystal/wiki">```wiki```</a> for more detailed information and tutorials.

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

| Packages | Nuget | License |
| :--- | :--- | :---|
|<a href="https://www.nuget.org/packages/Crystal.Core/">![](https://img.shields.io/badge/Crystal-Core-blue)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Core)](https://www.nuget.org/packages/Crystal.Core/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) |
|<a href="https://www.nuget.org/packages/Crystal.Desktop/">![](https://img.shields.io/badge/Crystal-Desktop-blue)</a> | [![NuGet Badge](https://buildstats.info/nuget/Crystal.Desktop)](https://www.nuget.org/packages/Crystal.Desktop/) | ![License: MIT](https://img.shields.io/badge/license-MIT-blue) 

[```Solid Principles```](https://github.com/jinhuca/Crystal/wiki/01.-Solid-Principles)
[```Composite Approach```](https://github.com/jinhuca/Crystal/wiki/02.-Composite-Approach)
[```Key Concepts```](https://github.com/jinhuca/Crystal/wiki/03.-Key-Concepts)
[```Application and Shell```](https://github.com/jinhuca/Crystal/wiki/04.-Application-and-Shell)
[```Modules```](https://github.com/jinhuca/Crystal/wiki/05.-Modules)
[```Regions```](https://github.com/jinhuca/Crystal/wiki/06.-Regions)
[```MVVM```](https://github.com/jinhuca/Crystal/wiki/07.-MVVM)
[```Commands```](https://github.com/jinhuca/Crystal/wiki/08.-Commands)
[```Event Aggregators```](https://github.com/jinhuca/Crystal/wiki/09.-Event-Aggregators)
<br/>
