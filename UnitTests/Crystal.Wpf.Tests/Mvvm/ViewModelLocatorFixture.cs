﻿using System;
using System.Reflection;
using Xunit;
using Crystal.Wpf.Tests.Mocks.ViewModels;
using Crystal.Wpf.Tests.Mocks.Views;

namespace Crystal.Wpf.Tests.Mvvm
{
  public class ViewModelLocatorFixture
  {
    [StaFact]
    public void ShouldLocateViewModelWithDefaultSettings()
    {
      ResetViewModelLocationProvider();

      Mock view = new Mock();
      Assert.Null(view.DataContext);

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      Assert.IsType<MockViewModel>(view.DataContext);
    }

    [StaFact]
    public void ShouldLocateViewModelWithDefaultSettingsForViewsThatEndWithView()
    {
      ResetViewModelLocationProvider();

      MockView view = new MockView();
      Assert.Null(view.DataContext);

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      Assert.IsType<MockViewModel>(view.DataContext);
    }

    [StaFact]
    public void ShouldUseCustomDefaultViewModelFactoryWhenSet()
    {
      ResetViewModelLocationProvider();

      Mock view = new Mock();
      Assert.Null(view.DataContext);

      object mockObject = new object();
      ViewModelLocationProvider.SetDefaultViewModelFactory(viewType => mockObject);

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      ReferenceEquals(view.DataContext, mockObject);
    }

    [StaFact]
    public void ShouldUseCustomDefaultViewTypeToViewModelTypeResolverWhenSet()
    {
      ResetViewModelLocationProvider();

      Mock view = new Mock();
      Assert.Null(view.DataContext);

      ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType => typeof(ViewModelLocatorFixture));

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      Assert.IsType<ViewModelLocatorFixture>(view.DataContext);
    }

    [StaFact]
    public void ShouldUseCustomFactoryWhenSet()
    {
      ResetViewModelLocationProvider();

      Mock view = new Mock();
      Assert.Null(view.DataContext);

      string viewModel = "Test String";
      ViewModelLocationProvider.Register(view.GetType().ToString(), () => viewModel);

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      ReferenceEquals(view.DataContext, viewModel);
    }

    internal static void ResetViewModelLocationProvider()
    {
      Type staticType = typeof(ViewModelLocationProvider);
      ConstructorInfo ci = staticType.TypeInitializer;
      object[] parameters = new object[0];
      ci.Invoke(null, parameters);
    }
  }
}
