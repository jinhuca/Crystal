﻿using Crystal;

namespace ModuleA.ViewModels
{
  public class ViewBViewModel : BindableBase, INavigationAware
  {
    private string _title = "ViewB";
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    private int _pageViews;
    public int PageViews
    {
      get => _pageViews;
      set => SetProperty(ref _pageViews, value);
    }

    public ViewBViewModel()
    {

    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      PageViews++;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {

    }
  }
}
