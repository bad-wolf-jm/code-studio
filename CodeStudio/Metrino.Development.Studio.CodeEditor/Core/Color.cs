using System;
using System.Drawing;

namespace Metrino.Development.Core
{
    public class Color
    {
        public int R { get; set; } = 0;
        public int G { get; set; } = 0;
        public int B { get; set; } = 0;
        public int A { get; set; } = 0;

        public Color() { }

        public static implicit operator Color(int[] values)
        {
            return new Color(values[0], values[1], values[2], values[3]);
        }

        public static implicit operator Color(float[] values)
        {
            return new Color(Convert.ToInt32(values[0] * 255), Convert.ToInt32(values[1] * 255), Convert.ToInt32(values[2] * 255), Convert.ToInt32(values[3] * 255));
        }

        public static implicit operator Color(double[] values)
        {
            return new Color(Convert.ToInt32(values[0] * 255), Convert.ToInt32(values[1] * 255), Convert.ToInt32(values[2] * 255), Convert.ToInt32(values[3] * 255));
        }

        public Color(Color color, int alpha)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = alpha;
        }

        public Color(int r, int g, int b, int alpha)
        {
            R = r;
            G = g;
            B = b;
            A = alpha;
        }

        public static Color FromArgb(int alpha, int r, int g, int b)
        {
            return new Color(r, g, b, alpha);
        }

        public static Color FromArgb(int alpha, Color color)
        {
            return new Color(color, alpha);
        }

        public static Color FromHex(string hex)
        {
            var color = ColorTranslator.FromHtml(hex);

            return new Color(color.R, color.G, color.B, color.A);
        }

        public override bool Equals(object? obj)
        {
            return obj is Color color && R == color.R && G == color.G && B == color.B && A == color.A;
        }

       public override int GetHashCode()
        {
            int h1 = R.GetHashCode();
            int h2 = G.GetHashCode();
            int h3 = B.GetHashCode();
            int h4 = A.GetHashCode();
            return h1 ^ h2 ^ h3 ^ h4;//HashCode.Combine(R, G, B, A);
        }
     }
}
