using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace WhipStat.Helpers
{
    public static class ColorUtilities
    {
        public static Color FromString(string str)
        {
            var converter = TypeDescriptor.GetConverter(typeof(Color));
            return (Color)converter.ConvertFromString(str);
        }

        public static Color GetIndexedColorOnGradient(double value, Color from, Color to)
        {
            value = Math.Clamp(value, 0, 1);

            var red = from.R + (int)(value * (to.R - from.R));
            var green = from.G + (int)(value * (to.G - from.G));
            var blue = from.B + (int)(value * (to.B - from.B));

            return Color.FromArgb(red, green, blue);
        }

        public static string GetIndexedColorOnGradient(double value, string from, string to)
        {
            var result = GetIndexedColorOnGradient(value, FromString(from), FromString(to));

            return $"#{result.R:X2}{result.G:X2}{result.B:X2}";
        }
    }
}
