using Crystal;
using Xunit;

namespace Crystal.Wpf.Tests.Modularity
{

	public class ModuleInfoGroupFixture
	{
		[Fact]
		public void ShouldForwardValuesToModuleInfo()
		{
			ModuleInfoGroup group = new ModuleInfoGroup();
			group.Ref = "MyCustomGroupRef";
			ModuleInfo moduleInfo = new ModuleInfo();
			Assert.Null(moduleInfo.Ref);

			group.Add(moduleInfo);

			Assert.Equal(group.Ref, moduleInfo.Ref);
		}
	}
}
