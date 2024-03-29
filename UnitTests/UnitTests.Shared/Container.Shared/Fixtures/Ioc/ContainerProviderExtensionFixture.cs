﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Container.Shared.Fixtures.Ioc
{
  public class ContainerProviderExtensionFixture : IDisposable
  {
    private static readonly MockService _unnamedService = new MockService();
    private static readonly IReadOnlyDictionary<string, MockService> _namedServiceDictionary = new Dictionary<string, MockService>
    {
      ["A"] = new MockService(),
      ["B"] = new MockService(),
    };

    private static readonly IContainerExtension _containerExtension
         = ContainerHelper.CreateContainerExtension();

    static ContainerProviderExtensionFixture()
    {
      _containerExtension.RegisterInstance<IService>(_unnamedService);
      foreach (var kvp in _namedServiceDictionary)
      {
        _containerExtension.RegisterInstance<IService>(kvp.Value, kvp.Key);
      }
      _containerExtension.FinalizeExtension();
    }

    public ContainerProviderExtensionFixture()
    {
      ContainerLocator.ResetContainer();
      ContainerLocator.SetContainerExtension(() => _containerExtension);
    }

    public void Dispose()
    {
      ContainerLocator.ResetContainer();
    }

    [Fact]
    public void CanResolveUnnamedServiceUsingConstructor()
    {
      var containerProvider = new ContainerProviderExtension(typeof(IService));
      var service = containerProvider.ProvideValue(null);

      Assert.Same(_unnamedService, service);
    }

    [Fact]
    public void CanResolveUnnamedServiceUsingProperty()
    {
      var containerProvider = new ContainerProviderExtension
      {
        Type = typeof(IService)
      };
      var service = containerProvider.ProvideValue(null);

      Assert.Same(_unnamedService, service);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public void CanResolvedNamedServiceUsingConstructor(string name)
    {
      var expectedService = _namedServiceDictionary[name];

      var containerProvider = new ContainerProviderExtension(typeof(IService))
      {
        Name = name,
      };
      var service = containerProvider.ProvideValue(null);

      Assert.Same(expectedService, service);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public void CanResolvedNamedServiceUsingProperty(string name)
    {
      var expectedService = _namedServiceDictionary[name];

      var containerProvider = new ContainerProviderExtension()
      {
        Type = typeof(IService),
        Name = name,
      };
      var service = containerProvider.ProvideValue(null);

      Assert.Same(expectedService, service);
    }

    [Theory]
    [InlineData(_xamlWithMarkupExtension)]
    [InlineData(_xamlWithXmlElement)]
    public void CanResolveServiceFromXaml(string xaml)
    {
      // Don't use StaTheoryAttribute. 
      // If use StaTheoryAttribute, ContainerLocator.Current will be changed by other test method
      // and Window.DataContext will be null.

      object dataContext = null;
      var thread = new Thread(() =>
      {
        using (var reader = new StringReader(xaml))
        {
          var window = XamlServices.Load(reader) as Window;
          dataContext = window.DataContext;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();

      Assert.Same(_unnamedService, dataContext);
    }
  }
}
