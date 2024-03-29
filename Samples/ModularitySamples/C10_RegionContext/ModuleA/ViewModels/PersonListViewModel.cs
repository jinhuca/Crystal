﻿using Crystal;
using ModuleA.Models;
using System.Collections.ObjectModel;

namespace ModuleA.ViewModels
{
  public class PersonListViewModel : BindableBase
  {
    private ObservableCollection<Person> _people;
    public ObservableCollection<Person> People
    {
      get => _people;
      set => SetProperty(ref _people, value);
    }

    public PersonListViewModel(ObservableCollection<Person> people)
    {
      _people = people;
      CreatePeople();
    }

    private void CreatePeople()
    {
      var people = new ObservableCollection<Person>();
      for (int i = 0; i < 10; i++)
      {
        people.Add(new Person()
        {
          FirstName = $"First {i}",
          LastName = $"Last {i}",
          Age = i
        });
      }

      People = people;
    }
  }
}
