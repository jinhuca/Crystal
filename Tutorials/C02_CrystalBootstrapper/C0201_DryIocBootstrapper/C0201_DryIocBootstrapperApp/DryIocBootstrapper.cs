using Crystal;
using Crystal.DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace C0201_DryIocBootstrapperApp
{
  public class DryIocBootstrapper : CrystalBootstrapper
  {
    protected override DependencyObject CreateShell()
    {
      return Container.Resolve<Shell>();
    }
  }
}
