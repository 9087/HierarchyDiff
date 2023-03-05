using GLib;
using Gtk;
using HierarchyDiff.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HierarchyDiff
{
    public class HierarchyModel : GLib.Object
    {
        private TreeStore treeModel;

        public TreeStore TreeModel
        {
            get => treeModel;
            set => treeModel = value;
        }

        public HierarchyModel()
        {
            treeModel = new TreeStore(typeof(Value));
        }

        public void LoadFromDocument(Document? document)
        {
            if (document == null)
            {
                throw new ArgumentNullException();
            }
            Queue<Tuple<TreeIter?, HierarchyDiff.Core.Node>> queue = new();
            queue.Enqueue(new(null, document.RootNode));
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var node = current.Item2;
                TreeIter treeIter = current.Item1 == null
                    ? treeModel.AppendNodes(node)
                    : treeModel.AppendNodes((TreeIter)current.Item1, node);
                foreach (var child in node)
                {
                    Debug.Assert(child != null);
                    queue.Enqueue(new(treeIter, child));
                }
            }
        }

        public TreeIter AppendNodes(TreeIter iter, params HierarchyDiff.Core.Node[] nodes)
        {
            return this.TreeModel.AppendValues(iter, nodes);
        }

        public TreeIter AppendNodes(params HierarchyDiff.Core.Node[] nodes)
        {
            return this.TreeModel.AppendValues(nodes);
        }

        public TreeIter? Find(HierarchyDiff.Core.Node node)
        {
            return this.TreeModel.Find(node);
        }

        public TreeIter? FindPartialNode(HierarchyDiff.Core.ParallelNode node)
        {
            return this.TreeModel.FindPartialNode(node);
        }

        public TreePath GetPath(TreeIter iter)
        {
            return this.TreeModel.GetPath(iter);
        }

        public bool GetIter(out TreeIter iter, TreePath path)
        {
            return this.TreeModel.GetIter(out iter, path);
        }

        public HierarchyDiff.Core.Node? GetNode(TreeIter iter, int column)
        {
            return this.TreeModel.GetNode(iter, column);
        }
    }
}
