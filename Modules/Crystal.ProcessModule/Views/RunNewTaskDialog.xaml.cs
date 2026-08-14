using System.Windows;
using System.Windows.Input;

namespace Crystal.ProcessModule.Views;

/// <summary>Modal input box for "Run new task": collects a command line and an elevation flag, then
/// returns them via <see cref="DialogResult"/>. It only gathers input — launching the process is the
/// view model's job.</summary>
public partial class RunNewTaskDialog : Window {
  public RunNewTaskDialog() {
    InitializeComponent();
    Loaded += (_, _) => CommandBox.Focus();
  }

  /// <summary>The command line the user entered (executable path or a name on PATH).</summary>
  public string Command => CommandBox.Text;

  /// <summary>Whether the user asked to launch elevated.</summary>
  public bool RunAsAdmin => AdminCheck.IsChecked == true;

  private void OnRun(object sender, RoutedEventArgs e) {
    if (string.IsNullOrWhiteSpace(CommandBox.Text)) return;
    DialogResult = true;
  }

  // Enter in the text box runs (IsDefault handles this too, but this keeps it explicit and lets an
  // empty command stay put instead of dismissing the dialog).
  private void OnCommandKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(CommandBox.Text)) {
      DialogResult = true;
    }
  }
}
