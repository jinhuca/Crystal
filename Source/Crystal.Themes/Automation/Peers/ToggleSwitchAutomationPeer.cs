using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace Crystal.Themes.Automation.Peers;

public class ToggleSwitchAutomationPeer : FrameworkElementAutomationPeer, IToggleProvider
{
  public ToggleSwitchAutomationPeer([NotNull] ToggleSwitch owner)
    : base(owner)
  {
  }

  protected override string GetClassNameCore()
  {
    return "ToggleSwitch";
  }

  /// <inheritdoc/>
  protected override AutomationControlType GetAutomationControlTypeCore()
  {
    return AutomationControlType.Button;
  }

  public override object? GetPattern(PatternInterface patternInterface)
  {
    return patternInterface == PatternInterface.Toggle ? this : base.GetPattern(patternInterface);
  }

  // BUG 1555137: Never inline, as we don't want to unnecessarily link the automation DLL
  [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
  internal virtual void RaiseToggleStatePropertyChangedEvent(bool oldValue, bool newValue)
  {
    if (oldValue != newValue)
    {
      RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, ConvertToToggleState(oldValue), ConvertToToggleState(newValue));
    }
  }

  private static ToggleState ConvertToToggleState(bool value)
  {
    return value ? ToggleState.On : ToggleState.Off;
  }

  public ToggleState ToggleState => ConvertToToggleState(((ToggleSwitch)Owner).IsOn);

  public void Toggle()
  {
    if (IsEnabled())
    {
      ((ToggleSwitch)Owner).AutomationPeerToggle();
    }
  }
}