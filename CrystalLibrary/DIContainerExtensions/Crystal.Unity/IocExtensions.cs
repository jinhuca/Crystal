using Unity;

namespace Crystal.Unity
{
	/// <summary>
	/// Extensions help get the underlying <see cref="IUnityContainer" />
	/// </summary>
	public static class IocExtensions
	{
		/// <summary>
		/// Gets the <see cref="IUnityContainer" /> from the <see cref="IContainerProvider"/>
		/// </summary>
		/// <param name="containerProvider">The current <see cref="IContainerProvider"/></param>
		/// <returns>The underlying <see cref="IUnityContainer"/></returns>
		public static IUnityContainer GetContainer(this IContainerProvider containerProvider) 
			=> ((IContainerExtension<IUnityContainer>)containerProvider).Instance;

		/// <summary>
		/// Gets the <see cref="IUnityContainer" /> from the <see cref="IContainerProvider" />
		/// </summary>
		/// <param name="containerRegistry">The current <see cref="IContainerRegistry" /></param>
		/// <returns>The underlying <see cref="IUnityContainer" /></returns>
		public static IUnityContainer GetContainer(this IContainerRegistry containerRegistry) 
			=> ((IContainerExtension<IUnityContainer>)containerRegistry).Instance;
	}
}
