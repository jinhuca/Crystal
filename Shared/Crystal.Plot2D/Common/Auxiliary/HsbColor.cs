using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Represents color in Hue Saturation Brightness color space.
/// </summary>
[SuppressMessage(category: "Microsoft.Naming", checkId: "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hsb")]
[DebuggerDisplay(value: "HSBColor A={Alpha} H={Hue} S={Saturation} B={Brightness}")]
public struct HsbColor {
  private double hue;
  private double saturation;
  private double brightness;
  private double alpha;

  /// <summary>Hue; [0, 360]</summary>
  public double Hue {
    get => hue;
    set {
      if(value < 0) {
        value = 360 - value;
      }

      hue = value % 360;
    }
  }

  /// <summary>Saturation; [0, 1]</summary>
  public double Saturation {
    get => saturation;
    set => saturation = value;
  }

  /// <summary>Brightness; [0, 1]</summary>
  public double Brightness {
    get => brightness;
    set => brightness = value;
  }

  /// <summary>Alpha; [0, 1]</summary>
  public double Alpha {
    get => alpha;
    set => alpha = value;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HSBColor"/> struct.
  /// </summary>
  /// <param name="hue">The hue; [0; 360]</param>
  /// <param name="saturation">The saturation; [0, 1]</param>
  /// <param name="brightness">The brightness; [0, 1]</param>
  public HsbColor(double hue, double saturation, double brightness) {
    this.hue = hue;
    this.saturation = saturation;
    this.brightness = brightness;
    alpha = 1;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HSBColor"/> struct.
  /// </summary>
  /// <param name="hue">The hue; [0, 360]</param>
  /// <param name="saturation">The saturation; [0, 1]</param>
  /// <param name="brightness">The brightness; [0, 1]</param>
  /// <param name="alpha">The alpha; [0, 1]</param>
  public HsbColor(double hue, double saturation, double brightness, double alpha) {
    this.hue = hue;
    this.saturation = saturation;
    this.brightness = brightness;
    this.alpha = alpha;
  }

  /// <summary>
  /// Creates HSBColor from the ARGB color.
  /// </summary>
  /// <param name="color">The color.</param>
  /// <returns></returns>
  public static HsbColor FromArgbColor(Color color) {
    const double limit255 = 255;

    var r_ = color.R / limit255;
    var g_ = color.G / limit255;
    var b_ = color.B / limit255;

    var max_ = Math.Max(val1: Math.Max(val1: r_, val2: g_), val2: b_);
    var min_ = Math.Min(val1: Math.Min(val1: r_, val2: g_), val2: b_);

    var len_ = max_ - min_;

    var brightness_ = max_; // 0.5 * (max + min);
    double sat_;
    double hue_;


    if(max_ == 0 || len_ == 0) {
      sat_ = hue_ = 0;
    }
    else {
      sat_ = len_ / max_;
      if(Math.Abs(r_ - max_) < Constants.Constants.FloatComparisonTolerance) {
        hue_ = (g_ - b_) / len_;
      }
      else if(Math.Abs(g_ - max_) < Constants.Constants.FloatComparisonTolerance) {
        hue_ = 2 + (b_ - r_) / len_;
      }
      else {
        hue_ = 4 + (r_ - g_) / len_;
      }
    }

    hue_ *= 60;
    if(hue_ < 0) {
      hue_ += 360;
    }

    HsbColor res_ = new() {
      hue = hue_,
      saturation = sat_,
      brightness = brightness_,
      alpha = color.A / limit255
    };
    return res_;
  }

  public static HsbColor FromArgb(int argb) {
    var a_ = (byte)(argb >> 24);
    var r_ = (byte)((argb >> 16) & 0xFF);
    var g_ = (byte)((argb >> 8) & 0xFF);
    var b_ = (byte)(argb & 0xFF);
    return FromArgbColor(color: Color.FromArgb(a: a_, r: r_, g: g_, b: b_));
  }

  /// <summary>
  /// Converts HSBColor to ARGB color space.
  /// </summary>
  /// <returns></returns>
  public Color ToArgbColor() {
    var r_ = 0.0;
    var g_ = 0.0;
    var b_ = 0.0;
    var hue_ = hue % 360.0;
    if(saturation == 0.0) {
      r_ = g_ = b_ = brightness;
    }
    else {
      var smallHue_ = hue_ / 60.0;
      var smallHueInt_ = (int)Math.Floor(d: smallHue_);
      var smallHueFrac_ = smallHue_ - smallHueInt_;
      var val1_ = brightness * (1.0 - saturation);
      var val2_ = brightness * (1.0 - saturation * smallHueFrac_);
      var val3_ = brightness * (1.0 - saturation * (1.0 - smallHueFrac_));
      switch(smallHueInt_) {
        case 0:
          r_ = brightness;
          g_ = val3_;
          b_ = val1_;
          break;

        case 1:
          r_ = val2_;
          g_ = brightness;
          b_ = val1_;
          break;

        case 2:
          r_ = val1_;
          g_ = brightness;
          b_ = val3_;
          break;

        case 3:
          r_ = val1_;
          g_ = val2_;
          b_ = brightness;
          break;

        case 4:
          r_ = val3_;
          g_ = val1_;
          b_ = brightness;
          break;

        case 5:
          r_ = brightness;
          g_ = val1_;
          b_ = val2_;
          break;
      }
    }


    return Color.FromArgb(
      a: (byte)Math.Round(a: alpha * 255),
      r: (byte)Math.Round(a: r_ * 255),
      g: (byte)Math.Round(a: g_ * 255),
      b: (byte)Math.Round(a: b_ * 255));
  }

  public int ToArgb() {
    return ToArgbColor().ToArgb();
  }

  /// <summary>
  /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
  /// </summary>
  /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
  /// <returns>
  /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
  /// </returns>
  public override bool Equals(object obj) {
    if(obj is HsbColor) {
      var c_ = (HsbColor)obj;
      return c_.alpha == alpha &&
             c_.brightness == brightness &&
             c_.hue == hue &&
             c_.saturation == saturation;
    }

    return false;
  }

  /// <summary>
  /// Returns a hash code for this instance.
  /// </summary>
  /// <returns>
  /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
  /// </returns>
  public override int GetHashCode() {
    return alpha.GetHashCode() ^
      brightness.GetHashCode() ^
      hue.GetHashCode() ^
      saturation.GetHashCode();
  }

  /// <summary>
  /// Implements the operator ==.
  /// </summary>
  /// <param name="first">The first.</param>
  /// <param name="second">The second.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator ==(HsbColor first, HsbColor second) =>
    first.alpha == second.alpha &&
    first.brightness == second.brightness &&
    first.hue == second.hue &&
    first.saturation == second.saturation;

  /// <summary>
  /// Implements the operator !=.
  /// </summary>
  /// <param name="first">The first.</param>
  /// <param name="second">The second.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator !=(HsbColor first, HsbColor second) =>
    first.alpha != second.alpha ||
    first.brightness != second.brightness ||
    first.hue != second.hue ||
    first.saturation != second.saturation;
}

public static class ColorExtensions {
  /// <summary>
  /// Converts the ARGB color to the HSB color.
  /// </summary>
  /// <param name="color">The color.</param>
  /// <returns></returns>
  [SuppressMessage(category: "Microsoft.Naming", checkId: "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hsb")]
  public static HsbColor ToHsbColor(this Color color) {
    return HsbColor.FromArgbColor(color: color);
  }
}
