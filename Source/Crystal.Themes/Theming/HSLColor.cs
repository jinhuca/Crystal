using Crystal.Themes.Standard;

#nullable enable
namespace Crystal.Themes.Theming
{
  /// <summary>
  /// This struct represent a Color in HSL (Hue, Saturation, Luminance)
  /// 
  /// Idea taken from here http://ciintelligence.blogspot.com/2012/02/converting-excel-theme-color-and-tint.html
  /// and here: https://en.wikipedia.org/wiki/HSL_and_HSV
  /// </summary>
  public struct HSLColor
    {
        /// <summary>
        /// Creates a new HSL Color
        /// </summary>
        /// <param name="color">Any System.Windows.Media.Color</param>
        public HSLColor(Color color)
        {
            // Init Parameters
            A = 0;
            H = 0;
            L = 0;
            S = 0;

            var r = color.R;
            var g = color.G;
            var b = color.B;
            var a = color.A;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));

            var delta = max - min;

            // Calculate H
            if (delta == 0)
            {
                H = 0;
            }
            else if (r == max)
            {
                H = 60 * (((double)(g - b) / delta) % 6);
            }
            else if (g == max)
            {
                H = 60 * (((double)(b - r) / delta) + 2);
            }
            else if (b == max)
            {
                H = 60 * (((double)(r - g) / delta) + 4);
            }

            if (H < 0)
            {
                H += 360;
            }

            // Calculate L 
            L = (1d / 2d * (max + min)) / 255d;

            // Calculate S
            if (DoubleUtilities.AreClose(L, 0) || DoubleUtilities.AreClose(L, 1))
            {
                S = 0;
            }
            else
            {
                S = delta / (255d * (1 - Math.Abs((2 * L) - 1)));
            }

            // Calculate Alpha
            A = a / 255d;
        }

        /// <summary>
        /// Creates a new HSL Color
        /// </summary>
        /// <param name="a">Alpha Channel [0;1]</param>
        /// <param name="h">Hue Channel [0;360]</param>
        /// <param name="s">Saturation Channel [0;1]</param>
        /// <param name="l">Luminance Channel [0;1]</param>
        public HSLColor(double a, double h, double s, double l)
        {
            A = a;
            H = h;
            S = s;
            L = l;
        }

        /// <summary>
        /// Gets or sets the Alpha channel.
        /// </summary>
        public double A { get; set; }

        /// <summary>
        /// Gets or sets the Hue channel.
        /// </summary>
        public double H { get; set; }

        /// <summary>
        /// Gets or sets the Saturation channel.
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// Gets or sets the Luminance channel.
        /// </summary>
        public double L { get; set; }

        /// <summary>
        /// Gets the ARGB-Color for this HSL-Color
        /// </summary>
        /// <returns>System.Windows.Media.Color</returns>
        public Color ToColor()
        {
            var r = GetColorComponent(0);
            var g = GetColorComponent(8);
            var b = GetColorComponent(4);
            return Color.FromArgb((byte)Math.Round(A * 255), r, g, b);
        }

        /// <summary>
        /// Gets a lighter / darker color based on a tint value. If <paramref name="tint"/> is > 0 then the returned color is darker, otherwise it will be lighter.
        /// </summary>
        /// <param name="tint">Tint Value in the Range [-1;1].</param>
        /// <returns>a new <see cref="Color"/> which is lighter or darker.</returns>
        public Color GetTintedColor(double tint)
        {
            var lum = L * 255;

            if (tint < 0)
            {
                lum *= 1.0 + tint;
            }
            else
            {
                lum = (lum * (1.0 - tint)) + (255 - (255 * (1.0 - tint)));
            }

            return new HSLColor(A, H, S, lum / 255d)
                .ToColor();
        }

        /// <summary>
        /// Gets a lighter / darker color based on a tint value. If <paramref name="tint"/> is > 0 then the returned color is darker, otherwise it will be lighter.
        /// </summary>
        /// <param name="color">The input color which should be tinted.</param>
        /// <param name="tint">Tint Value in the Range [-1;1].</param>
        /// <returns>a new <see cref="Color"/> which is lighter or darker.</returns>
        public static Color GetTintedColor(Color color, double tint)
        {
            return new HSLColor(color)
                .GetTintedColor(tint);
        }

        private byte GetColorComponent(int n)
        {
            double a = S * Math.Min(L, 1 - L);
            double k = (n + (H / 30)) % 12;

            return (byte)Math.Round(255 * (L - (a * Math.Max(-1, Math.Min(k - 3, Math.Min(9 - k, 1))))));
        }

        public override bool Equals(object? obj)
        {
            return obj is HSLColor color
                   && DoubleUtilities.AreClose(A, color.A)
                   && DoubleUtilities.AreClose(H, color.H)
                   && DoubleUtilities.AreClose(S, color.S)
                   && DoubleUtilities.AreClose(L, color.L);
        }

        public override int GetHashCode()
        {
            int hashCode = -1795249040;
            hashCode = (hashCode * -1521134295) + A.GetHashCode();
            hashCode = (hashCode * -1521134295) + H.GetHashCode();
            hashCode = (hashCode * -1521134295) + S.GetHashCode();
            hashCode = (hashCode * -1521134295) + L.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(HSLColor x, HSLColor y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(HSLColor x, HSLColor y)
        {
            return !(x == y);
        }
    }
}