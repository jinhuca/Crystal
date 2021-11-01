using Crystal;
using S0301ModuleA.Business;
using S0301ModuleA.ViewModels;

namespace S0301ModuleA.Views
{
	public partial class PersonDetail
    {
        public PersonDetail()
        {
            InitializeComponent();
            RegionContext.GetObservableContext(this).PropertyChanged += PersonDetail_PropertyChanged;
        }

        private void PersonDetail_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var context = (ObservableObject<object>)sender;
            var selectedPerson = (Person)context.Value;
            ((PersonDetailViewModel)DataContext).SelectedPerson = selectedPerson;
        }
    }
}
