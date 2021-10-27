namespace Crystal.Unity
{
	public static class StringConstants
	{
		public const string NotOverwrittenGetModuleEnumeratorException = "The method 'GetModuleEnumerator' of the bootstrapper must be overwritten in order to use the default module initialization logic.";
		public const string NullLoggerFacadeException = "The ILoggerFacade is required and cannot be null.";
		public const string NullModuleCatalogException = "The IModuleCatalog is required and cannot be null in order to initialize the modules.";
		public const string NullUnityContainerException = "The IUnityContainer is required and cannot be null.";
		public const string SettingTheRegionManager = "Setting the RegionManager.";
		public const string TypeMappingAlreadyRegistered = "Type '{0}' was already registered by the application. Skipping...";
	}
}
