using Crystal.Plot2D.Common;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

public delegate DataRect ViewportConstraintCallback(DataRect proposedDataRect);

public class InjectionDelegateConstraint : ViewportConstraint {
  public InjectionDelegateConstraint(Viewport2D masterViewport, ViewportConstraintCallback callback) {
    Callback = callback ?? throw new ArgumentNullException(paramName: nameof(callback));
    MasterViewport = masterViewport ?? throw new ArgumentNullException(paramName: nameof(masterViewport));
    masterViewport.PropertyChanged += MasterViewport_PropertyChanged;
  }

  private void MasterViewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    if(e.PropertyName == "Visible") {
      RaiseChanged();
    }
  }

  public ViewportConstraintCallback Callback { get; }
  public Viewport2D MasterViewport { get; set; }

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    return Callback(proposedDataRect: proposedDataRect);
  }
}
