﻿using System;
using System.ComponentModel;
using System.Reflection;
using Crystal.Constants;
using static System.String;

namespace Crystal;

/// <summary>
/// Represents each node of nested properties expression and takes care of 
/// subscribing/unsubscribing INotifyPropertyChanged.PropertyChanged listeners on it.
/// </summary>
internal class PropertyObserverNode
{
  private readonly Action _action;
  private INotifyPropertyChanged _inpcObject;

  public PropertyInfo PropertyInfo { get; }
  public PropertyObserverNode Next { get; set; }

  public PropertyObserverNode(PropertyInfo propertyInfo, Action action)
  {
    PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
    _action = () =>
    {
      action?.Invoke();
      if (Next == null)
      {
        return;
      }

      Next.UnsubscribeListener();
      GenerateNextNode();
    };
  }

  public void SubscribeListenerFor(INotifyPropertyChanged inpcObject)
  {
    _inpcObject = inpcObject;
    _inpcObject.PropertyChanged += OnPropertyChanged;

    if (Next != null)
    {
      GenerateNextNode();
    }
  }

  private void GenerateNextNode()
  {
    var nextProperty = PropertyInfo.GetValue(_inpcObject);
    if (nextProperty == null)
    {
      return;
    }

    if (nextProperty is not INotifyPropertyChanged nextInpcObject)
    {
      throw new InvalidOperationException(Format(StringConstants.NotImplementInpc, Next.PropertyInfo.Name));
    }
    Next.SubscribeListenerFor(nextInpcObject);
  }

  private void UnsubscribeListener()
  {
    if (_inpcObject != null)
    {
      _inpcObject.PropertyChanged -= OnPropertyChanged;
    }
    Next?.UnsubscribeListener();
  }

  private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (e?.PropertyName == PropertyInfo.Name || IsNullOrEmpty(e?.PropertyName))
    {
      _action?.Invoke();
    }
  }
}