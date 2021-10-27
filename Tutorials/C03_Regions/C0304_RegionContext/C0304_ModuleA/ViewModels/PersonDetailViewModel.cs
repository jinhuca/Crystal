using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C0304_ModuleA.Models;
using Crystal;

namespace C0304_ModuleA.ViewModels
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
