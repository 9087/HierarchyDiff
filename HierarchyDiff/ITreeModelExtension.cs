using GLib;
using Gtk;

namespace HierarchyDiff
{
    public static class ITreeModelExtension
    {
        public static HierarchyDiff.Core.Node? GetNode(this ITreeModel treeModel, TreeIter iter, int column)
        {
            var value = treeModel.GetValue(iter, column) as Value?;
            if (!value.HasValue)
            {
                return null;
            }
            var node = value.Value.Val as HierarchyDiff.Core.Node;
            return node;
        }

        public static bool IterBrother(this ITreeModel treeModel, ref TreeIter current)
        {
            TreeIter brother = current;
            if (!treeModel.IterNext(ref brother))
            {
                return false;
            }
            current = brother;
            return true;
        }
    }
}
