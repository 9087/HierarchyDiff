using Gdk;
using Gtk;
using HierarchyDiff.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace HierarchyDiff
{
    public class HierarchyView : Bin
    {
        #region Hierarchy tree view

        protected class TreeView : Gtk.TreeView
        {
            protected override bool OnButtonPressEvent(EventButton evnt)
            {
                // Avoid unexpected scrolling reset of ScrolledWindow after pressing TreeView
                return true;
            }
        }

        protected TreeView treeView;

        private HierarchyModel? model = null;

        public HierarchyModel? Model
        {
            get => model;
            set
            {
                model = value;
                treeView.Model = model?.TreeModel;
            }
        }

        public Action<HierarchyDiff.Core.Node>? NodeCollapsed;
        public Action<HierarchyDiff.Core.Node>? NodeExpanded;
        public Action<HashSet<HierarchyDiff.Core.Node>>? NodeSelectionChanged;
        protected HashSet<TreePath> selectedPaths = new();
        protected TreePath? firstPath;

        private static IEnumerable<TreeIter> Traverse(ITreeModel model, TreeIter current, bool first = true)
        {
            while (true)
            {
                yield return current;
                if (model.IterChildren(out TreeIter child, current))
                {
                    foreach (var iter in Traverse(model, child, false))
                    {
                        yield return iter;
                    }
                }
                if (model.IterBrother(ref current))
                {
                    continue;
                }
                if (!first)
                {
                    goto finish;
                }
                while (true)
                {
                    if (!model.IterParent(out var parent, current))
                    {
                        goto finish;
                    }
                    current = parent;
                    if (model.IterBrother(ref current))
                    {
                        break;
                    }
                }
            }
        finish:;
        }

        private void SelectRange(TreePath firstPath, TreePath lastPath)
        {
            Model!.GetIter(out var first, firstPath);
            Model.GetIter(out var last, lastPath);
            HashSet<TreeIter> iters = new();
            bool ok = false;
            if (!ok)
            {
                iters.Clear();
                foreach (var iter in Traverse(Model.TreeModel, first))
                {
                    iters.Add(iter);
                    if (iter.Equals(last))
                    {
                        ok = true;
                        break;
                    }
                }
            }
            if (!ok)
            {
                iters.Clear();
                var tmp = first;
                first = last;
                last = tmp;
                foreach (var iter in Traverse(Model.TreeModel, first))
                {
                    iters.Add(iter);
                    if (iter.Equals(last))
                    {
                        ok = true;
                        break;
                    }
                }
            }
            var paths = new HashSet<TreePath>();
            if (ok)
            {
                foreach (var iter_ in iters)
                {
                    paths.Add(Model.GetPath(iter_));
                }
            }
            SelectPaths(paths);
        }

        Gdk.Window? lastButtonPressedWindow = null;

        [GLib.ConnectBefore]
        private void OnButtonPressed(object o, ButtonPressEventArgs args)
        {
            var @event = args.Event;
            lastButtonPressedWindow = @event.Window;
            treeView.GetPathAtPos((int)@event.X, (int)@event.Y, out var path);
            if (path == null)
            {
                firstPath = null;
                SelectPaths(new HashSet<TreePath>());
                return;
            }
            else if (@event.Button == 1)
            {
                TreePath lastPath;
                if (firstPath != null && @event.State.HasFlag(Gdk.ModifierType.ShiftMask))
                {
                    lastPath = path;
                }
                else
                {
                    lastPath = firstPath = path;
                }
                SelectRange(firstPath, lastPath);
            }
        }

        private void OnButtonReleased(object o, ButtonReleaseEventArgs args)
        {
            var @event = args.Event;
            if (lastButtonPressedWindow != @event.Window)
            {
                return;
            }
            this.treeView.GetPathAtPos((int)@event.X, (int)@event.Y, out var path);
            var cellArea = this.treeView.GetCellArea(path, this.treeView.Columns[0]);
            if (@event.X < cellArea.X)
            {
                if (Model!.GetIter(out var iter, path))
                {
                    var node = Model.GetNode(iter, 0);
                    if (treeView.GetRowExpanded(path))
                    {
                        NodeCollapsed?.Invoke(node!);
                    }
                    else
                    {
                        NodeExpanded?.Invoke(node!);
                    }
                }
            }
        }

        public void SelectPaths(HashSet<TreePath> paths)
        {
            if (selectedPaths.SetEquals(paths))
            {
                return;
            }
            HashSet<TreePath> changedPaths = new();
            var treeModel = model!.TreeModel;
            foreach (var path in selectedPaths)
            {
                changedPaths.Add(path);
            }
            selectedPaths.Clear();
            foreach (var path in paths)
            {
                if (selectedPaths.Contains(path))
                {
                    continue;
                }
                selectedPaths.Add(path);
                changedPaths.Add(path);
            }
            OnSelectionChanged(this, EventArgs.Empty);
            foreach (var path in changedPaths)
            {
                treeModel.GetIter(out var iter, path);
                treeModel.EmitRowChanged(path, iter);
            }
        }

        public bool IterIsSelected(TreeIter iter)
        {
            return selectedPaths.Contains(this.Model!.GetPath(iter));
        }

        public TreePath[] GetSelectedRows()
        {
            return selectedPaths.ToArray();
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            var selectedRows = this.GetSelectedRows();
            Debug.Assert(treeView.Selection.Mode == SelectionMode.None);
            HashSet<HierarchyDiff.Core.Node> nodes = new();
            foreach (var treePath in selectedRows)
            {
                if (!this.Model!.GetIter(out var iter, treePath))
                {
                    continue;
                }
                var node = Model.GetNode(iter, 0);
                nodes.Add(node!);
            }
            NodeSelectionChanged?.Invoke(nodes);
        }

        private void OnRowCollapsed(object o, RowCollapsedArgs args)
        {
            var node = Model!.GetNode(args.Iter, 0);
            NodeCollapsed?.Invoke(node!);
        }

        private void OnRowExpanded(object o, RowExpandedArgs args)
        {
            var node = Model!.GetNode(args.Iter, 0);
            NodeExpanded?.Invoke(node!);
        }

        public bool ExpandRow(TreePath path, bool openAll)
        {
            return treeView.ExpandRow(path, openAll);
        }

        public bool CollapseRow(TreePath path)
        {
            return treeView.CollapseRow(path);
        }

        public void ExpandAll()
        {
            treeView.ExpandAll();
        }

        #endregion

        public HierarchyView()
        {
            // Hierarchy tree view
            Add(treeView = new());
            treeView.AppendColumn(new HierarchyViewColumnName("Name"));
            treeView.AppendColumn(new HierarchyViewColumnValue("Value"));
            treeView.RowExpanded += OnRowExpanded;
            treeView.RowCollapsed += OnRowCollapsed;
            treeView.Selection.Changed += OnSelectionChanged;
            treeView.Selection.Mode = SelectionMode.None;
            treeView.ButtonReleaseEvent += OnButtonReleased;
            treeView.ButtonPressEvent += OnButtonPressed;
        }
    }
}
