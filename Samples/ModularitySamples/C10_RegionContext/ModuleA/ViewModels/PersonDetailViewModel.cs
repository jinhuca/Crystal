using Crystal;
using ModuleA.Models;

namespace ModuleA.ViewModels
{
  public class PersonDetailViewModel : BindableBase
  {
    private Person _selectedPerson;
    public Person SelectedPerson
    {
      get => _selectedPerson;
      set => SetProperty(ref _selectedPerson, value);
    }

    public PersonDetailViewModel()
    {

    }
  }
}
