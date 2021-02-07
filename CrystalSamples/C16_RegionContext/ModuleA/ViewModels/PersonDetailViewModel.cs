using Crystal.Mvvm;
using ModuleA.Business;

namespace ModuleA.ViewModels
{
	public class PersonDetailViewModel : BindableBase
	{
		private Person _selectedPerson;
		public Person SelectedPerson
		{
			get { return _selectedPerson; }
			set { SetProperty(ref _selectedPerson, value); }
		}

		public PersonDetailViewModel()
		{
		}
	}
}
