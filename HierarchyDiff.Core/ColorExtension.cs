using System.Drawing;

namespace HierarchyDiff
{
    public static class ColorExtension
    {
        public static string ToRgbCode(this Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        public static string ToRgbaCode(this Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.R, color.G, color.B, color.A);
        }
    }
}
