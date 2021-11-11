﻿using CompositeCommandShared;
using Crystal;
using System;

namespace ModuleA.ViewModels
{
  internal class TabViewModel : BindableBase
  {
    IApplicationCommands _applicationCommands;

    private string _title = string.Empty;
    public string Title
    {
      get { return _title; }
      set { SetProperty(ref _title, value); }
    }

    private bool _canUpdate = true;
    public bool CanUpdate
    {
      get { return _canUpdate; }
      set { SetProperty(ref _canUpdate, value); }
    }

    private string _updatedText = string.Empty;
    public string UpdateText
    {
      get { return _updatedText; }
      set { SetProperty(ref _updatedText, value); }
    }

    public DelegateCommand UpdateCommand { get; set; }

    public TabViewModel(IApplicationCommands applicationCommands)
    {
      _applicationCommands = applicationCommands;
      UpdateCommand = new DelegateCommand(Update).ObservesCanExecute(() => CanUpdate);
      _applicationCommands.SaveCommand.RegisterCommand(UpdateCommand);
    }

    private void Update() => UpdateText = $"Updated: {DateTime.Now}";
  }
}
