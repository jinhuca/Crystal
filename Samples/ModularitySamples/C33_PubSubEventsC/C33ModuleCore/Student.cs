using Crystal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C33ModuleCore
{
  public class Student : BindableBase
  {
    private int _age;
    public int Age
    {
      get => _age;
      set => SetProperty(ref _age, value);
    }

    private string _name;
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    private double _gdp;
    public double Gdp
    {
      get => _gdp;
      set => SetProperty(ref _gdp, value);
    }
  }
}
