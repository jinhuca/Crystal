using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.DryIoc
{
	public static class StringConstants
	{
		public const string NotOverwrittenGetModuleEnumeratorException = "The method 'GetModuleEnumerator' of the bootstrapper must be overwritten in order to use the default module initialization logic.";
		public const string NullDryIocContainerBuilderException = "The ContainerBuilder is required and cannot be null.";
		public const string NullDryIocContainerException = "The IContainer is required and cannot be null.";
		public const string NullLoggerFacadeException = "The ILoggerFacade is required and cannot be null.";
		public const string NullModuleCatalogException = "The IModuleCatalog is required and cannot be null in order to initialize the modules.";
		public const string TypeMappingAlreadyRegistered = "Type '{0}' was already registered by the application. Skipping...";
	}
}
