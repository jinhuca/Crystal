using System.Windows.Controls;

namespace Crystal.Shell.Startup;

/// <summary>The startup loading overlay. Its <see cref="LoadingViewModel"/> is assigned
/// explicitly by <c>App</c> because it takes constructor arguments.</summary>
public partial class LoadingView : UserControl {
  public LoadingView() {
    InitializeComponent();
  }
}
