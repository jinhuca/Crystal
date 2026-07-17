using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.ComponentModel;
using System.Windows;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Contains data for custom generation of tick's label.
/// </summary>
/// <typeparam name="T">Type of ticks</typeparam>
public sealed class LabelTickInfo<T> {
  internal LabelTickInfo() { }

  /// <summary>
  /// Gets or sets the tick.
  /// </summary>
  /// <value>The tick.</value>
  public T Tick { get; internal set; }
  /// <summary>
  /// Gets or sets additional info about ticks range.
  /// </summary>
  /// <value>The info.</value>
  public object Info { get; internal set; }
  /// <summary>
  /// Gets or sets the index of tick in ticks array.
  /// </summary>
  /// <value>The index.</value>
  public int Index { get; internal set; }
}

/// <summary>
/// Base class for all label providers.
/// Contains a number of properties that can be used to adjust generated labels.
/// </summary>
/// <typeparam name="T">Type of ticks, which labels are generated for</typeparam>
/// <remarks>
/// Order of application of custom label string properties:
/// If CustomFormatter is not null, it is called first.
/// Then, if it was null or if it returned null string,
/// virtual GetStringCore method is called. It can be overloaded in subclasses. GetStringCore should not return null.
/// Then if LabelStringFormat is not null, it is applied.
/// After label's UI was created, you can change it by setting CustomView delegate - it allows you to adjust 
/// UI properties of label. Note: not all labelProviders takes CustomView into account.
/// </remarks>
public abstract class LabelProviderBase<T> {
  #region Private

  private string labelStringFormat;
  private Func<LabelTickInfo<T>, string> customFormatter;
  private Action<LabelTickInfo<T>, UIElement> customView;

  #endregion

  protected static UIElement[] EmptyLabelsArray { get; } = Array.Empty<UIElement>();

  /// <summary>
  /// Creates labels by given ticks info.
  /// Is not intended to be called from your code.
  /// </summary>
  /// <param name="ticksInfo">The ticks info.</param>
  /// <returns>Array of <see cref="UIElement"/>s, which are axis labels for specified axis ticks.</returns>
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public abstract UIElement[] CreateLabels(ITicksInfo<T> ticksInfo);

  /// <summary>
  /// Gets or sets the label string format.
  /// </summary>
  /// <value>The label string format.</value>
  public string LabelStringFormat {
    get => labelStringFormat;
    set {
      if(labelStringFormat != value) {
        labelStringFormat = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Gets or sets the custom formatter - delegate that can be called to create custom string representation of tick.
  /// </summary>
  /// <value>The custom formatter.</value>
  public Func<LabelTickInfo<T>, string> CustomFormatter {
    get => customFormatter;
    set {
      if(customFormatter != value) {
        customFormatter = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Gets or sets the custom view - delegate that is used to create a custom, non-default look of axis label.
  /// Can be used to adjust some UI properties of generated label.
  /// </summary>
  /// <value>The custom view.</value>
  public Action<LabelTickInfo<T>, UIElement> CustomView {
    get => customView;
    set {
      if(customView != value) {
        customView = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Sets the custom formatter.
  /// This is alternative to CustomFormatter property setter, the only difference is that Visual Studio shows 
  /// more convenient tooltip for methods rather than for properties' setters.
  /// </summary>
  /// <param name="formatter">The formatter.</param>
  public void SetCustomFormatter(Func<LabelTickInfo<T>, string> formatter) {
    CustomFormatter = formatter;
  }

  /// <summary>
  /// Sets the custom view.
  /// This is alternative to CustomView property setter, the only difference is that Visual Studio shows 
  /// more convenient tooltip for methods rather than for properties' setters.
  /// </summary>
  /// <param name="view">The view.</param>
  public void SetCustomView(Action<LabelTickInfo<T>, UIElement> view) {
    CustomView = view;
  }

  protected string GetString(LabelTickInfo<T> tickInfo) {
    string text = null;
    if(CustomFormatter != null) {
      text = CustomFormatter(arg: tickInfo);
    }
    if(text == null) {
      text = GetStringCore(tickInfo: tickInfo);

      if(text == null) {
        throw new ArgumentNullException(paramName: Strings.Exceptions.TextOfTickShouldNotBeNull);
      }
    }
    if(LabelStringFormat != null) {
      text = string.Format(format: LabelStringFormat, arg0: text);
    }

    return text;
  }

  protected virtual string GetStringCore(LabelTickInfo<T> tickInfo) {
    return tickInfo.Tick.ToString();
  }

  protected void ApplyCustomView(LabelTickInfo<T> info, UIElement label) {
    if(CustomView != null) {
      CustomView(arg1: info, arg2: label);
    }
  }

  /// <summary>
  /// Occurs when label provider is changed.
  /// Notifies axis to update its view.
  /// </summary>
  public event EventHandler Changed;
  protected void RaiseChanged() {
    Changed.Raise(sender: this);
  }

  private readonly ResourcePool<UIElement> pool = new();
  internal void ReleaseLabel(UIElement label) {
    if(ReleaseCore(label: label)) {
      pool.Put(item: label);
    }
  }

  protected virtual bool ReleaseCore(UIElement label) { return false; }

  protected UIElement GetResourceFromPool() {
    return pool.Get();
  }
}
