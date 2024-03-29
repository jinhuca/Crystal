using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Crystal.UnitTests.Mvvm
{
  public class ErrorsContainerFixture
  {
    [Fact]
    public void WhenCreatingAnInstanceWithANullAction_ThenAnExceptionIsThrown()
    {
      Assert.Throws<ArgumentNullException>(() => new ErrorsContainer<object>(null));
    }

    [Fact]
    public void WhenCreatingInstance_ThenHasNoErrors()
    {
      var validation = new ErrorsContainer<string>(pn => { });

      Assert.False(validation.HasErrors);
      Assert.False(validation.GetErrors("property1").Any());
    }

    [Fact]
    public void WhenSettingErrorsForPropertyWithNoErrors_ThenNotifiesChangesAndHasErrors()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });

      Assert.True(validation.HasErrors);
      Assert.Contains("message", validation.GetErrors("property1"));
      Assert.Equal(new[] { "property1" }, validatedProperties);
    }

    [Fact]
    public void WhenSettingNoErrorsForPropertyWithNoErrors_ThenDoesNotNotifyChangesAndHasNoErrors()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new string[0]);

      Assert.False(validation.HasErrors);
      Assert.False(validation.GetErrors("property1").Any());
      Assert.False(validatedProperties.Any());
    }

    [Fact]
    public void WhenSettingErrorsForPropertyWithErrors_ThenNotifiesChangesAndHasErrors()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });

      validatedProperties.Clear();

      validation.SetErrors("property1", new[] { "message" });

      Assert.True(validation.HasErrors);
      Assert.Contains("message", validation.GetErrors("property1"));
      Assert.Equal(new[] { "property1" }, validatedProperties);
    }

    [Fact]
    public void WhenSettingNoErrorsForPropertyWithErrors_ThenNotifiesChangesAndHasNoErrors()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });

      validatedProperties.Clear();

      validation.SetErrors("property1", new string[0]);

      Assert.False(validation.HasErrors);
      Assert.False(validation.GetErrors("property1").Any());
      Assert.Equal(new[] { "property1" }, validatedProperties);
    }

    [Fact]
    public void WhenClearingErrorsForPropertyWithErrors_ThenPropertyHasNoErrors()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });
      validation.SetErrors("property2", new[] { "message2" });

      validation.ClearErrors("property1");

      Assert.True(validation.HasErrors);
      Assert.False(validation.GetErrors("property1").Any());
      Assert.True(validation.GetErrors("property2").Any());
    }

    [Fact]
    public void WhenClearingErrorsWithNullPropertyName_ThenHasErrors()
    {
      List<string> validatedProperties = new();
      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));
      validation.SetErrors("property1", new[] { "message" });
      validation.ClearErrors(null);
      Assert.True(validation.HasErrors);
      Assert.True(validation.GetErrors("property1").Any());
    }

    [Fact]
    public void WhenClearingErrorsForPropertyWithErrorsGeneric_ThenPropertyHasNoErrors()
    {
      var viewModel = new Mocks.ViewModels.MockValidatingViewModel { MockProperty = -5 };
      Assert.True(viewModel.HasErrors);
      viewModel.ClearMockPropertyErrors();
      Assert.False(viewModel.HasErrors);
    }

    [Fact]
    public void WhenGettingErrors_ThenErrorsPerPropertyReturned()
    {
      List<string> validatedProperties = new();
      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));
      validation.SetErrors("property1", new[] { "message" });
      validation.SetErrors("property2", new[] { "message" });
      var errors = validation.GetErrors();

      Assert.True(errors.Any());
      Assert.True(errors.Count == 2);
      Assert.Contains(errors, e => e.Key.Equals("property1"));
      Assert.Contains(errors, e => e.Key.Equals("property2"));
    }

    [Fact]
    public void WhenGettingErrorsWithPropertyName_ThenErrorsReturned()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });

      var errors = validation.GetErrors("property1");

      Assert.True(errors.Any());
    }

    [Fact]
    public void WhenGettingErrorsWithNullPropertyName_ThenNoErrorsReturned()
    {
      List<string> validatedProperties = new();

      var validation = new ErrorsContainer<string>(pn => validatedProperties.Add(pn));

      validation.SetErrors("property1", new[] { "message" });

      var errors = validation.GetErrors(null);

      Assert.True(validation.HasErrors);
      Assert.True(!errors.Any());
    }

    [Fact]
    public void WhenSettingsErrorsForPropertyWithNullCollection_ThenPropertyHasNoErrors()
    {
      var viewModel = new Mocks.ViewModels.MockValidatingViewModel { MockProperty = 10 };
      Assert.False(viewModel.HasErrors);

      viewModel.SetMockPropertyErrorsWithNullCollection();

      Assert.False(viewModel.HasErrors);
    }
  }
}
