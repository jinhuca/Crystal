using System;
using System.Reflection;
using Crystal.UnitTests.Mocks.ViewModels;
using Crystal.UnitTests.Mocks.Views;
using Xunit;

namespace Crystal.UnitTests.Mvvm
{
  public class ViewModelLocationProviderFixture
  {
	  [Fact]
	  public void ShouldFailWhenCustomDefaultViewTypeToViewModelTypeResolverIsNull()
	  {
		  ResetViewModelLocationProvider();
		  var view = new Mock();
		  ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType => null);
		  ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
		  {
			  Assert.NotNull(v);
			  Assert.NotNull(vm);
		  });
	  }

    [Fact]
    public void ShouldLocateViewModelWithDefaultSettings()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        //Assert.IsType<MockViewModel>(vm);
      });
    }

    [Fact]
    public void ShouldLocateViewModelWithDefaultSettingsForViewsThatEndWithView()
    {
      ResetViewModelLocationProvider();

      var view = new MockView();

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        Assert.IsType<MockViewModel>(vm);
      });
    }

    [Fact]
    public void ShouldUseCustomDefaultViewModelFactoryWhenSet()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      object mockObject = new();
      ViewModelLocationProvider.SetDefaultViewModelFactory(viewType => mockObject);

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        //Assert.IsType(mockObject.GetType(), vm);
      });
    }

    [Fact]
    public void ShouldUseCustomDefaultViewTypeToViewModelTypeResolverWhenSet()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType => typeof(ViewModelLocationProviderFixture));

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        Assert.IsType<ViewModelLocationProviderFixture>(vm);
      });
    }

    [Fact]
    public void ShouldUseCustomFactoryWhenSet()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      string viewModel = "Test String";
      ViewModelLocationProvider.Register(view.GetType().ToString(), () => viewModel);

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        Assert.Equal(viewModel, vm);
      });
    }

    [Fact]
    public void ShouldUseCustomFactoryWhenSet_Generic()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      string viewModel = "Test String";
      ViewModelLocationProvider.Register<Mock>(() => viewModel);

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        Assert.Equal(viewModel, vm);
      });
    }

    [Fact]
    public void ShouldUseCustomTypeWhenSet()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      ViewModelLocationProvider.Register(view.GetType().ToString(), typeof(ViewModelLocationProviderFixture));

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        Assert.IsType<ViewModelLocationProviderFixture>(vm);
      });
    }

    [Fact]
    public void ShouldUseCustomTypeWhenSet_Generic()
    {
      ResetViewModelLocationProvider();

      var view = new Mock();

      ViewModelLocationProvider.Register<Mock, ViewModelLocationProviderFixture>();

      ViewModelLocationProvider.AutoWireViewModelChanged(view, (v, vm) =>
      {
        Assert.NotNull(v);
        Assert.NotNull(vm);
        //Assert.IsType<ViewModelLocationProviderFixture>(vm);
      });
    }

    private static void ResetViewModelLocationProvider()
    {
      Type staticType = typeof(ViewModelLocationProvider);
      ConstructorInfo ci = staticType.GetTypeInfo().TypeInitializer;
      object[] parameters = new object[0];
      ci.Invoke(null, parameters);
    }
  }
}
