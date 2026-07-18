using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Interfaces; 
public interface IMmiProvider {
  public IMmiQuery GetQueryProvider(MmiCategory category);
}
