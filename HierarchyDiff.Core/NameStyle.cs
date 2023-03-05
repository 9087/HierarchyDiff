using System.Drawing;

namespace HierarchyDiff.Core
{
    public class Style
    {
        public Color? BackgroundColor { get; set; } = null;

        public Color? TextColor { get; set; } = null;

        public int? Padding { get; set; } = null;

        public bool? Italic { get; set; } = null;

        public bool? Bold { get; set; } = null;

        public string? Format { get; set; } = null;
    }
}
