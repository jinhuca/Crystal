using System;
using System.Windows.Input;
using Crystal;
using Xunit;

namespace Crystal.UnitTests.Commands
{
	public class DelegateCommandFixture : BindableBase
	{
		[Fact]
		public void WhenConstructedWithoutGenericTypeOfObject_InitializesValues()
		{
			// Prepare

			// Act
			var actual = new DelegateCommand<object>(param => { });

			// Assert
			Assert.NotNull(actual);
		}

		[Fact]
		public void WhenConstructedWithGenericTypeOfNullable_InitializesValues()
		{
			// Prepare

			// Act
			var actual = new DelegateCommand<int?>(param => { });

			// Assert
			Assert.NotNull(actual);
		}

		[Fact]
		public void WhenContructedWithGenericTypeIsNonNullableValueType_Throws()
		{
			Assert.Throws<InvalidCastException>(() =>
			{
				var actual = new DelegateCommand<int>(param => { });
			});
		}

		[Fact]
		public void ExecuteCallsPassedInExecuteDelegate()
		{
			var handlers = new DelegateHandlers();
			var command = new DelegateCommand<object>(handlers.Execute);
			object parameter = new object();
			command.Execute(parameter);
			Assert.Same(parameter, handlers.ExecuteParameter);
		}

		class DelegateHandlers
		{
			public bool CanExecuteReturnValue = true;
			public object CanExecuteParameter;
			public object ExecuteParameter;

			public bool CanExecute(object parameter)
			{
				CanExecuteParameter = parameter;
				return CanExecuteReturnValue;
			}

			public void Execute(object parameter)
			{
				ExecuteParameter = parameter;
			}
		}
	}
}
