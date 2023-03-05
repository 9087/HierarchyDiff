using System.Drawing;

namespace HierarchyDiff
{
    public static class ColorExtension
    {
        public static string ToRgbCode(this Color color)
        {
            return string.Format(
                "{0:D2}{1:D2}{2:D2}",
                color.R.ToString("X"),
                color.G.ToString("X"),
                color.B.ToString("X"));
        }

        public static string ToRgbaCode(this Color color)
        {
            return string.Format(
                "{0:D2}{1:D2}{2:D2}{3:D2}",
                color.R.ToString("X"),
                color.G.ToString("X"),
                color.B.ToString("X"),
                color.A.ToString("X"));
        }
    }
}
