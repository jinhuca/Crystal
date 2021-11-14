using Crystal;
using ModuleA.Models;
using ModuleA.ViewModels;

namespace ModuleA.Views
{
  public partial class PersonDetail
  {
    public PersonDetail()
    {
      InitializeComponent();
      RegionContext.GetObservableContext(this).PropertyChanged += PersonDetail_PropertyChanged;
    }

    private void PersonDetail_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      var context = (ObservableObject<object>)sender;
      var selectedPerson = (Person)context.Value;
      (DataContext as PersonDetailViewModel).SelectedPerson = selectedPerson;
    }
  }
}
