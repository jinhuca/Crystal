using System;
using System.Collections.Generic;
using System.Text;

namespace Container.Shared.ViewModels
{
  public class ConstructorArgumentViewModel : BindableBase
  {
    public IServiceA Service { get; }

    public ConstructorArgumentViewModel(IServiceA service)
    {
      Service = service;
    }
  }
}
