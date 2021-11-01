using Crystal;
using S0301ModuleA.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S0301ModuleA.ViewModels
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
