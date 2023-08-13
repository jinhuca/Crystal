using System;
using System.Windows;

namespace Crystal
{
	/// <summary>
	/// Base class that provides a basic bootstrapping sequence and hooks that specific implementations can override.
	/// </summary>
	/// <remarks>
	/// This class must be overridden to provide application specific configuration.
	/// </remarks>
	[Obsolete]
  public abstract class CrystalBootstrapperBase
	{
		IContainerExtension _containerExtension;
		IModuleCatalog _moduleCatalog;

		/// <summary>
		/// The dependency injection container used to resolve objects
		/// </summary>
		public IContainerProvider Container => _containerExtension;

		/// <summary>
		/// Gets the shell user interface
		/// </summary>
		/// <value>The shell user interface.</value>
		protected DependencyObject Shell { get; set; }

		/// <summary>
		/// Runs the bootstrapper process.
		/// </summary>
		public void Run()
		{
			ConfigureViewModelLocator();
			Initialize();
			OnInitialized();
		}

#pragma warning disable CS1574 // XML comment has cref attribute 'ViewModelLocator' that could not be resolved
		/// <summary>
		/// Configures the <see cref="Crystal.Mvvm.ViewModelLocator"/> used by Crystal.
		/// </summary>
		protected virtual void ConfigureViewModelLocator() => CrystalInitializationExtensions.ConfigureViewModelLocator();
#pragma warning restore CS1574 // XML comment has cref attribute 'ViewModelLocator' that could not be resolved

		/// <summary>
		/// Runs the initialization sequence to configure the Crystal application.
		/// </summary>
		protected virtual void Initialize()
		{
			ContainerLocator.SetContainerExtension(CreateContainerExtension);
			_containerExtension = ContainerLocator.Current;
			_moduleCatalog = CreateModuleCatalog();
			RegisterRequiredTypes(_containerExtension);
			RegisterTypes(_containerExtension);
			_containerExtension.FinalizeExtension();
			ConfigureModuleCatalog(_moduleCatalog);

			var regionAdapterMappings = _containerExtension.Resolve<RegionAdapterMappings>();
			ConfigureRegionAdapterMappings(regionAdapterMappings);

			var defaultRegionBehaviors = _containerExtension.Resolve<IRegionBehaviorFactory>();
			ConfigureDefaultRegionBehaviors(defaultRegionBehaviors);
			RegisterFrameworkExceptionTypes();

			var shell = CreateShell();
			if (shell != null)
			{
				MvvmHelpers.AutowireViewModel(shell);
				RegionManager.SetRegionManager(shell, _containerExtension.Resolve<IRegionManager>());
				RegionManager.UpdateRegions();
				InitializeShell(shell);
			}

			InitializeModules();
		}

		/// <summary>
		/// Creates the container used by Crystal.
		/// </summary>
		/// <returns>The container</returns>
		protected abstract IContainerExtension CreateContainerExtension();

		/// <summary>
		/// Creates the <see cref="IModuleCatalog"/> used by Crystal.
		/// </summary>
		///  <remarks>
		/// The base implementation returns a new ModuleCatalog.
		/// </remarks>
		protected virtual IModuleCatalog CreateModuleCatalog() => new ModuleCatalog();

		/// <summary>
		/// Registers all types that are required by Crystal to function with the container.
		/// </summary>
		/// <param name="containerRegistry"></param>
		protected virtual void RegisterRequiredTypes(IContainerRegistry containerRegistry)
		{
			if (_moduleCatalog == null)
			{
				throw new InvalidOperationException("IModuleCatalog");
			}
			containerRegistry.RegisterRequiredTypes(_moduleCatalog);
		}

		/// <summary>
		/// Used to register types with the container that will be used by your application.
		/// </summary>
		protected virtual void RegisterTypes(IContainerRegistry containerRegistry) { }

		/// <summary>
		/// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
		/// root exceptions by the <see cref="ExceptionExtensions"/>.
		/// </summary>
		protected virtual void RegisterFrameworkExceptionTypes()
		{
		}

		/// <summary>
		/// Configures the <see cref="IRegionBehaviorFactory"/>. 
		/// This will be the list of default behaviors that will be added to a region. 
		/// </summary>
		protected virtual void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors)
			=> regionBehaviors?.RegisterDefaultRegionBehaviors();

		/// <summary>
		/// Configures the default region adapter mappings to use in the application, in order
		/// to adapt UI controls defined in XAML to use a region and register it automatically.
		/// May be overwritten in a derived class to add specific mappings required by the application.
		/// </summary>
		/// <returns>The <see cref="RegionAdapterMappings"/> instance containing all the mappings.</returns>
		protected virtual void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
			=> regionAdapterMappings?.RegisterDefaultRegionAdapterMappings();

		/// <summary>
		/// Creates the shell or main window of the application.
		/// </summary>
		/// <returns>The shell of the application.</returns>
		protected abstract DependencyObject CreateShell();

		/// <summary>
		/// Initializes the shell.
		/// </summary>
		protected virtual void InitializeShell(DependencyObject shell) => Shell = shell;

		/// <summary>
		/// Contains actions that should occur last.
		/// </summary>
		protected virtual void OnInitialized()
		{
			if (Shell is Window window)
			{
				window.Show();
			}
		}

		/// <summary>
		/// Configures the <see cref="IModuleCatalog"/> used by Crystal.
		/// </summary>
		protected virtual void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) { }

		/// <summary>
		/// Initializes the modules.
		/// </summary>
		protected virtual void InitializeModules() => CrystalInitializationExtensions.RunModuleManager(Container);
	}
}
