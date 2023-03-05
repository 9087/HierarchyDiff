using Gdk;

namespace HierarchyDiff
{
    public class HierarchyViewColumnValue : HierarchyViewColumn
    {
        public HierarchyViewColumnValue(string title) : base(title)
        {
        }

        protected override Pixbuf? OnRequestIcon(CellInfo cellInfo)
        {
            return null;
        }

        protected override string OnRequestText(CellInfo cellInfo)
        {
            textRenderer.Strikethrough = false;
            textRenderer.Underline = Pango.Underline.None;
            if (!object.Equals(cellInfo.Node.Current?.Value, cellInfo.Node.Other?.Value))
            {
                if (cellInfo.Node.Index == 0)
                {
                    textRenderer.Strikethrough = true;
                }
                else
                {
                    textRenderer.Underline = Pango.Underline.Single;
                }
            }
            if (cellInfo.Node.Current == null)
            {
                return string.Empty;
            }
            var value = cellInfo.Node.Value;
            return value == null ? "" : HierarchyDiff.Utility.Dump(value);
        }
    }
}