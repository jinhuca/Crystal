using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;
using static Crystal.Constants.StringConstants;

namespace Crystal
{
	/// <summary>
	/// Component responsible for coordinating the modules' type loading and module initialization process.
	/// </summary>
	public partial class ModuleManager : IModuleManager, IDisposable
	{
		private readonly IModuleInitializer moduleInitializer;
		private IEnumerable<IModuleTypeLoader> typeLoaders;
		private readonly HashSet<IModuleTypeLoader> subscribedToModuleTypeLoaders = new();

		/// <summary>
		/// Initializes an instance of the <see cref="ModuleManager"/> class.
		/// </summary>
		/// <param name="moduleInitializer">Service used for initialization of modules.</param>
		/// <param name="moduleCatalog">Catalog that enumerates the modules to be loaded and initialized.</param>
		public ModuleManager(IModuleInitializer moduleInitializer, IModuleCatalog moduleCatalog)
		{
			this.moduleInitializer = moduleInitializer ?? throw new ArgumentNullException(nameof(moduleInitializer));
			ModuleCatalog = moduleCatalog ?? throw new ArgumentNullException(nameof(moduleCatalog));
		}

		/// <summary>
		/// The module catalog specified in the constructor.
		/// </summary>
		protected IModuleCatalog ModuleCatalog { get; }

		/// <summary>
		/// Gets all the <see cref="IModuleInfo"/> classes that are in the <see cref="IModuleCatalog"/>.
		/// </summary>
		public IEnumerable<IModuleInfo> Modules => ModuleCatalog.Modules;

		/// <summary>
		/// Raised repeatedly to provide progress as modules are loaded in the background.
		/// </summary>
		public event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

		private void RaiseModuleDownloadProgressChanged(ModuleDownloadProgressChangedEventArgs e)
		{
			ModuleDownloadProgressChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Raised when a module is loaded or fails to load.
		/// </summary>
		public event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;

		private void RaiseLoadModuleCompleted(IModuleInfo moduleInfo, Exception error)
		{
			RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
		}

		private void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
		{
			LoadModuleCompleted?.Invoke(this, e);
		}

		/// <summary>
		/// Initializes the modules marked as <see cref="InitializationMode.WhenAvailable"/> on the <see cref="ModuleCatalog"/>.
		/// </summary>
		public void Run()
		{
			ModuleCatalog.Initialize();

			LoadModulesWhenAvailable();
		}


		/// <summary>
		/// Loads and initializes the module on the <see cref="IModuleCatalog"/> with the name <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module requested for initialization.</param>
		public void LoadModule(string moduleName)
		{
			var module = ModuleCatalog.Modules.Where(m => m.ModuleName == moduleName);
			if (module == null || module.Count() != 1)
			{
				throw new ModuleNotFoundException(moduleName, Format(ModuleNotFound, moduleName));
			}

			var modulesToLoad = ModuleCatalog.CompleteListWithDependencies(module);

			LoadModuleTypes(modulesToLoad);
		}

		/// <summary>
		/// Checks if the module needs to be retrieved before it's initialized.
		/// </summary>
		/// <param name="moduleInfo">Module that is being checked if needs retrieval.</param>
		/// <returns></returns>
		protected virtual bool ModuleNeedsRetrieval(IModuleInfo moduleInfo)
		{
			if (moduleInfo == null)
			{
				throw new ArgumentNullException(nameof(moduleInfo));
			}

			if (moduleInfo.State == ModuleState.NotStarted)
			{
				// If we can instantiate the type, that means the module's assembly is already loaded into
				// the AppDomain and we don't need to retrieve it.
				bool isAvailable = Type.GetType(moduleInfo.ModuleType) != null;
				if (isAvailable)
				{
					moduleInfo.State = ModuleState.ReadyForInitialization;
				}

				return !isAvailable;
			}

			return false;
		}

		private void LoadModulesWhenAvailable()
		{
			var whenAvailableModules = ModuleCatalog.Modules.Where(m => m.InitializationMode == InitializationMode.WhenAvailable);
			var modulesToLoadTypes = ModuleCatalog.CompleteListWithDependencies(whenAvailableModules);
			if (modulesToLoadTypes != null)
			{
				LoadModuleTypes(modulesToLoadTypes);
			}
		}

		private void LoadModuleTypes(IEnumerable<IModuleInfo> moduleInfos)
		{
			if (moduleInfos == null)
			{
				return;
			}

			foreach (var moduleInfo in moduleInfos)
			{
				if (moduleInfo.State == ModuleState.NotStarted)
				{
					if (ModuleNeedsRetrieval(moduleInfo))
					{
						BeginRetrievingModule(moduleInfo);
					}
					else
					{
						moduleInfo.State = ModuleState.ReadyForInitialization;
					}
				}
			}

			LoadModulesThatAreReadyForLoad();
		}

		/// <summary>
		/// Loads the modules that are not initialized and have their dependencies loaded.
		/// </summary>
		protected virtual void LoadModulesThatAreReadyForLoad()
		{
			bool keepLoading = true;
			while (keepLoading)
			{
				keepLoading = false;
				var availableModules = ModuleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

				foreach (var moduleInfo in availableModules)
				{
					if ((moduleInfo.State != ModuleState.Initialized) && (AreDependenciesLoaded(moduleInfo)))
					{
						moduleInfo.State = ModuleState.Initializing;
						InitializeModule(moduleInfo);
						keepLoading = true;
						break;
					}
				}
			}
		}

		private void BeginRetrievingModule(IModuleInfo moduleInfo)
		{
			var moduleInfoToLoadType = moduleInfo;
			IModuleTypeLoader moduleTypeLoader = GetTypeLoaderForModule(moduleInfoToLoadType);
			moduleInfoToLoadType.State = ModuleState.LoadingTypes;

			// Delegate += works differently between SL and WPF.
			// We only want to subscribe to each instance once.
			if (!subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
			{
				moduleTypeLoader.ModuleDownloadProgressChanged += IModuleTypeLoader_ModuleDownloadProgressChanged;
				moduleTypeLoader.LoadModuleCompleted += IModuleTypeLoader_LoadModuleCompleted;
				subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
			}

			moduleTypeLoader.LoadModuleType(moduleInfo);
		}

		private void IModuleTypeLoader_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
		{
			RaiseModuleDownloadProgressChanged(e);
		}

		private void IModuleTypeLoader_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
				{
					e.ModuleInfo.State = ModuleState.ReadyForInitialization;
				}

				// This callback may call back on the UI thread, but we are not guaranteeing it.
				// If you were to add a custom retriever that retrieved in the background, you
				// would need to consider dispatching to the UI thread.
				LoadModulesThatAreReadyForLoad();
			}
			else
			{
				RaiseLoadModuleCompleted(e);

				// If the error is not handled then I log it and raise an exception.
				if (!e.IsErrorHandled)
				{
					HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
				}
			}
		}

		/// <summary>
		/// Handles any exception occurred in the module typeloading process,
		/// and throws a <see cref="ModuleTypeLoadingException"/>.
		/// This method can be overridden to provide a different behavior.
		/// </summary>
		/// <param name="moduleInfo">The module metadata where the error happened.</param>
		/// <param name="exception">The exception thrown that is the cause of the current error.</param>
		/// <exception cref="ModuleTypeLoadingException"></exception>
		protected virtual void HandleModuleTypeLoadingError(IModuleInfo moduleInfo, Exception exception)
		{
			if (moduleInfo == null)
			{
				throw new ArgumentNullException(nameof(moduleInfo));
			}

			if (!(exception is ModuleTypeLoadingException moduleTypeLoadingException))
			{
				moduleTypeLoadingException = new ModuleTypeLoadingException(moduleInfo.ModuleName, exception.Message, exception);
			}

			throw moduleTypeLoadingException;
		}

		private bool AreDependenciesLoaded(IModuleInfo moduleInfo)
		{
			var requiredModules = ModuleCatalog.GetDependentModules(moduleInfo);
			if (requiredModules == null)
			{
				return true;
			}

			int notReadyRequiredModuleCount =
					requiredModules.Count(requiredModule => requiredModule.State != ModuleState.Initialized);

			return notReadyRequiredModuleCount == 0;
		}

		private IModuleTypeLoader GetTypeLoaderForModule(IModuleInfo moduleInfo)
		{
			foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
			{
				if (typeLoader.CanLoadModuleType(moduleInfo))
				{
					return typeLoader;
				}
			}

			throw new ModuleTypeLoaderNotFoundException(moduleInfo.ModuleName, Format(NoRetrieverCanRetrieveModule, moduleInfo.ModuleName), null);
		}

		private void InitializeModule(IModuleInfo moduleInfo)
		{
			if (moduleInfo.State == ModuleState.Initializing)
			{
				moduleInitializer.Initialize(moduleInfo);
				moduleInfo.State = ModuleState.Initialized;
				RaiseLoadModuleCompleted(moduleInfo, null);
			}
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes the associated <see cref="IModuleTypeLoader"/>s.
		/// </summary>
		/// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
		protected virtual void Dispose(bool disposing)
		{
			foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
			{
				if (typeLoader is IDisposable disposableTypeLoader)
				{
					disposableTypeLoader.Dispose();
				}
			}
		}

		#endregion
	}
}
