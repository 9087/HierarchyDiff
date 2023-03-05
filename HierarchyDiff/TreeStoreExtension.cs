using GLib;
using Gtk;
using HierarchyDiff.Core;
using System.Linq;

namespace HierarchyDiff
{
    public static class TreeStoreExtension
    {
        public static TreeIter AppendNodes(this TreeStore treeStore, TreeIter iter, params HierarchyDiff.Core.Node[] nodes)
        {
            return treeStore.AppendValues(iter, nodes.Select(x => new Value(x)).ToArray());
        }

        public static TreeIter AppendNodes(this TreeStore treeStore, params HierarchyDiff.Core.Node[] nodes)
        {
            return treeStore.AppendValues(nodes.Select(x => new Value(x)).ToArray());
        }

        public static TreeIter? Find(this TreeStore treeStore, HierarchyDiff.Core.Node node)
        {
            TreeIter? found = null;
            treeStore.Foreach((ITreeModel model, TreePath path, TreeIter iter) =>
            {
                if (model.GetNode(iter, 0) == node)
                {
                    found = iter;
                    return true;
                }
                return false;
            });
            return found;
        }

        public static TreeIter? FindPartialNode(this TreeStore treeStore, HierarchyDiff.Core.ParallelNode node)
        {
            TreeIter? found = null;
            treeStore.Foreach((ITreeModel model, TreePath path, TreeIter iter) =>
            {
                if (!(model.GetNode(iter, 0) is PartialNode partialNode))
                {
                    return false;
                }
                if (ReferenceEquals(partialNode.ParallelNode, node))
                {
                    found = iter;
                    return true;
                }
                return false;
            });
            return found;
        }
    }
}
