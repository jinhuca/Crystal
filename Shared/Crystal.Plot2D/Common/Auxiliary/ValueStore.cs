using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Crystal.Plot2D.Common.Auxiliary;

public sealed class ValueStore : CustomTypeDescriptor, INotifyPropertyChanged {
  public ValueStore(params string[] propertiesNames) {
    foreach(var propertyName in propertiesNames) {
      this[propertyName: propertyName] = "";
    }
  }

  private readonly Dictionary<string, object> cache = new();

  public object this[string propertyName] {
    get => cache[key: propertyName];
    set => SetValue(propertyName: propertyName, value: value);
  }

  public void SetValue(string propertyName, object value) {
    cache[key: propertyName] = value;
    PropertyChanged.Raise(sender: this, propertyName: propertyName);
  }

  private PropertyDescriptorCollection collection;
  public override PropertyDescriptorCollection GetProperties() {
    var propertyDescriptors = new PropertyDescriptor[cache.Count];
    var keys = cache.Keys.ToArray();
    for(var i = 0; i < keys.Length; i++) {
      propertyDescriptors[i] = new ValueStorePropertyDescriptor(name: keys[i]);
    }

    collection = new PropertyDescriptorCollection(properties: propertyDescriptors);
    return collection;
  }

  private sealed class ValueStorePropertyDescriptor : PropertyDescriptor {
    private readonly string name;

    public ValueStorePropertyDescriptor(string name)
      : base(name: name, attrs: null) {
      this.name = name;
    }

    public override bool CanResetValue(object component) {
      return false;
    }

    public override Type ComponentType => typeof(ValueStore);

    public override object GetValue(object component) {
      var store = (ValueStore)component;
      return store[propertyName: name];
    }

    public override bool IsReadOnly => false;

    public override Type PropertyType => typeof(string);

    public override void ResetValue(object component) {
    }

    public override void SetValue(object component, object value) {
    }

    public override bool ShouldSerializeValue(object component) {
      return false;
    }
  }

  #region INotifyPropertyChanged Members

  public event PropertyChangedEventHandler PropertyChanged;

  #endregion
}
