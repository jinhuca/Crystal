using System;
using System.Windows.Input;
using Xunit;

namespace Crystal.UnitTests.Commands
{
	public class DelegateCommandFixture : BindableBase
	{
		[Fact]
		public void WhenConstructedWithoutGenericTypeOfObject_InitializesValues()
		{
			var actual = new DelegateCommand<object>(_ => { });
			Assert.NotNull(actual);
		}

		[Fact]
		public void WhenConstructedWithGenericTypeOfNullable_InitializesValues()
		{
			var actual = new DelegateCommand<int?>(_ => { });
			Assert.NotNull(actual);
		}

		[Fact]
		public void WhenConstructedWithGenericTypeIsNonNullableValueType_Throws()
		{
			Assert.Throws<InvalidCastException>(() => { new DelegateCommand<int>(_ => { }); });
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

		[Fact]
		public void CanExecuteCallsPassedInCanExecuteDelegate()
		{
			var handlers = new DelegateHandlers();
			var command = new DelegateCommand<object>(handlers.Execute, handlers.CanExecute);
			object parameter = new object();

			handlers.CanExecuteReturnValue = true;
			bool retVal = command.CanExecute(parameter);

			Assert.Same(parameter, handlers.CanExecuteParameter);
			Assert.Equal(handlers.CanExecuteReturnValue, retVal);
		}

		[Fact]
		public void CanExecuteReturnsTrueWithoutCanExecuteDelegate()
		{
			var handlers = new DelegateHandlers();
			var command = new DelegateCommand<object>(handlers.Execute);
			bool retValue = command.CanExecute(null);
			Assert.True(retValue);
		}

		[Fact]
		public void RaiseCanExecuteChanged_RaisesCanExecuteChanged()
		{
			var handlers = new DelegateHandlers();
			var command = new DelegateCommand<object>(handlers.Execute);
			bool canExecuteChanged = false;
			command.CanExecuteChanged += delegate { canExecuteChanged = true; };
			command.RaiseCanExecuteChanged();
			Assert.True(canExecuteChanged);
		}

		[Fact]
		public void CanRemoveCanExecuteChangedHandler()
		{
			var command = new DelegateCommand<object>(_ => { });
			bool canExecuteChangedRaised = false;
			void Handler(object? s, EventArgs e) => canExecuteChangedRaised = true;
			command.CanExecuteChanged += Handler;
			command.CanExecuteChanged -= Handler;
			command.RaiseCanExecuteChanged();
			Assert.False(canExecuteChangedRaised);
		}

		[Fact]
		public void ShouldPassParameterInstanceOnExecute()
		{
			bool executeCalled = false;
			MyClass testClass = new MyClass();
			ICommand command = new DelegateCommand<MyClass>(delegate (MyClass parameter)
			{
				Assert.Same(testClass, parameter);
				executeCalled = true;
			});
			command.Execute(testClass);
			Assert.True(executeCalled);
		}

		[Fact]
		public void ShouldPassParameterInstanceOnCanExecute()
		{
			bool canExecuteCalled = false;
			MyClass testClass = new MyClass();
			ICommand command = new DelegateCommand<MyClass>(_ => { }, delegate (MyClass parameter)
				{
					Assert.Same(testClass, parameter);
					canExecuteCalled = true;
					return true;
				});
			command.CanExecute(testClass);
			Assert.True(canExecuteCalled);
		}

		[Fact]
		public void ShouldThrowIfAllDelegatesAreNull()
		{
			Assert.Throws<ArgumentNullException>(() => { new DelegateCommand<object>(null, null); });
		}

		[Fact]
		public void ShouldThrowIfExecuteMethodDelegateNull()
		{
			Assert.Throws<ArgumentNullException>(() => { new DelegateCommand<object>(null); });
		}

		[Fact]
		public void ShouldThrowIfCanExecuteMethodDelegateNull()
		{
			Assert.Throws<ArgumentNullException>(() => { new DelegateCommand<object>(obj => { }, null); });
		}

		[Fact]
		public void DelegateCommandShouldThrowIfAllDelegatesAreNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand(null, null);
			});
		}

		[Fact]
		public void DelegateCommandShouldThrowIfExecuteMethodDelegateNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand(null);
			});
		}

		[Fact]
		public void DelegateCommandGenericShouldThrowIfCanExecuteMethodDelegateNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand<object>(obj => { }, null);
			});
		}

		[Fact]
		public void IsActivePropertyIsFalseByDefault()
		{
			var command = new DelegateCommand<object>(DoNothing);
			Assert.False(command.IsActive);
		}

		[Fact]
		public void IsActivePropertyChangeFiresEvent()
		{
			bool fired = false;
			var command = new DelegateCommand<object>(DoNothing);
			command.IsActiveChanged += delegate { fired = true; };
			command.IsActive = true;
			Assert.True(fired);
		}

		[Fact]
		public void NonGenericDelegateCommandExecuteShouldInvokeExecuteAction()
		{
			bool executed = false;
			var command = new DelegateCommand(() => { executed = true; });
			command.Execute();
			Assert.True(executed);
		}

		[Fact]
		public void NonGenericDelegateCommandCanExecuteShouldInvokeCanExecuteFun()
		{
			bool invoked = false;
			var command = new DelegateCommand(() => { }, () =>
			{
				invoked = true;
				return true;
			});
			bool canExecute = command.CanExecute();
			Assert.True(invoked);
			Assert.True(canExecute);
		}


		[Fact]
		public void NonGenericDelegateCommandShouldDefaultCanExecuteToTrue()
		{
			var command = new DelegateCommand(() => { });
			Assert.True(command.CanExecute());
		}

		[Fact]
		public void NonGenericDelegateThrowsIfDelegatesAreNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand(null, null);
			});
		}

		[Fact]
		public void NonGenericDelegateCommandThrowsIfExecuteDelegateIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand(null);
			});
		}

		[Fact]
		public void NonGenericDelegateCommandThrowsIfCanExecuteDelegateIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var command = new DelegateCommand(() => { }, null);
			});
		}

		[Fact]
		public void NonGenericDelegateCommandShouldObserveCanExecute()
		{
			bool canExecuteChangedRaised = false;

			ICommand command = new DelegateCommand(() => { }).ObservesCanExecute(() => BoolProperty);

			command.CanExecuteChanged += delegate { canExecuteChangedRaised = true; };

			Assert.False(canExecuteChangedRaised);
			Assert.False(command.CanExecute(null));

			BoolProperty = true;

			Assert.True(canExecuteChangedRaised);
			Assert.True(command.CanExecute(null));
		}

		[Fact]
		public void NonGenericDelegateCommandShouldObserveCanExecuteAndObserveOtherProperties()
		{
			bool canExecuteChangedRaised = false;

			ICommand command = new DelegateCommand(() => { }).ObservesCanExecute(() => BoolProperty).ObservesProperty(() => IntProperty);

			command.CanExecuteChanged += delegate { canExecuteChangedRaised = true; };

			Assert.False(canExecuteChangedRaised);
			Assert.False(command.CanExecute(null));

			IntProperty = 10;

			Assert.True(canExecuteChangedRaised);
			Assert.False(command.CanExecute(null));

			canExecuteChangedRaised = false;
			Assert.False(canExecuteChangedRaised);

			BoolProperty = true;

			Assert.True(canExecuteChangedRaised);
			Assert.True(command.CanExecute(null));
		}

		[Fact]
		public void NonGenericDelegateCommandShouldNotObserveDuplicateCanExecute()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				ICommand command = new DelegateCommand(() => { }).ObservesCanExecute(() => BoolProperty).ObservesCanExecute(() => BoolProperty);
			});
		}

		[Fact]
		public void NonGenericDelegateCommandShouldObserveOneProperty()
		{
			bool canExecuteChangedRaised = false;

			var command = new DelegateCommand(() => { }).ObservesProperty(() => IntProperty);

			command.CanExecuteChanged += delegate { canExecuteChangedRaised = true; };

			IntProperty = 10;

			Assert.True(canExecuteChangedRaised);
		}

		[Fact]
		public void NonGenericDelegateCommandShouldObserveMultipleProperties()
		{
			bool canExecuteChangedRaised = false;

			var command = new DelegateCommand(() => { }).ObservesProperty(() => IntProperty).ObservesProperty(() => BoolProperty);

			command.CanExecuteChanged += delegate { canExecuteChangedRaised = true; };

			IntProperty = 10;

			Assert.True(canExecuteChangedRaised);

			canExecuteChangedRaised = false;

			BoolProperty = true;

			Assert.True(canExecuteChangedRaised);
		}

		internal void DoNothing(object param) { }

		public class ComplexType : TestPurposeBindableBase
		{
			private int _intProperty;
			public int IntProperty
			{
				get => _intProperty;
				set => SetProperty(ref _intProperty, value);
			}

			private ComplexType _innerComplexProperty;
			public ComplexType InnerComplexProperty
			{
				get => _innerComplexProperty;
				set => SetProperty(ref _innerComplexProperty, value);
			}
		}

		private ComplexType _complexProperty;
		public ComplexType ComplexProperty
		{
			get => _complexProperty;
			set => SetProperty(ref _complexProperty, value);
		}

		private bool _boolProperty;
		public bool BoolProperty
		{
			get => _boolProperty;
			set => SetProperty(ref _boolProperty, value);
		}

		private int _intProperty;
		public int IntProperty
		{
			get => _intProperty;
			set => SetProperty(ref _intProperty, value);
		}
	}

	internal class DelegateHandlers
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

	internal class MyClass { }
}
