using Gdk;
using Gtk;
using HierarchyDiff.Core;
using System.Diagnostics;
using System.Linq;
using static HierarchyDiff.HierarchyViewColumn;

namespace HierarchyDiff
{
    public class HierarchyViewColumnName : HierarchyViewColumn
    {
        public HierarchyViewColumnName(string title) : base(title)
        {
        }

        protected override Pixbuf? OnRequestIcon(CellInfo cellInfo)
        {
            if (cellInfo.Node.Current == null)
            {
                return null;
            }
            return cellInfo.Node.Icon.GetPixbuf();
        }

        protected override string OnRequestText(CellInfo cellInfo)
        {
            textRenderer.Strikethrough = false;
            textRenderer.Underline = Pango.Underline.None;
            if (!Name.Equals(cellInfo.Node.Current?.Name, cellInfo.Node.Other?.Name))
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
            return cellInfo.Node.Name.ToMarkup(cellInfo.Selected);
        }
    }
}