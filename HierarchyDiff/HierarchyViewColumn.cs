using Gdk;
using GLib;
using Gtk;
using HierarchyDiff.Core;
using System;
using System.Diagnostics;
using System.Linq;

namespace HierarchyDiff
{
    public abstract class HierarchyViewColumn : Gtk.TreeViewColumn
    {
        protected CellRendererPixbuf iconRenderer;
        protected CellRendererText textRenderer;

        public HierarchyViewColumn(string title)
        {
            this.PackStart(iconRenderer = new CellRendererPixbuf() {
                Xpad = 3,
            }, false);
            this.SetCellDataFunc(iconRenderer, OnRenderingInternal);
            this.PackStart(textRenderer = new CellRendererText() {
                Ellipsize = Pango.EllipsizeMode.End,
                Height = 26,
            }, false);
            this.SetCellDataFunc(textRenderer, OnRenderingInternal);
            this.Title = title;
            this.Resizable = true;
        }

        public class CellInfo
        {
            public HierarchyViewColumn Column { get; private set; }

            public CellRenderer CellRenderer { get; private set; }

            public ITreeModel Model { get; private set; }

            public TreeIter Iter { get; private set; }

            public HierarchyView HierarchyView { get; private set; }

            public bool Selected { get; private set; }

            public PartialNode Node { get; private set; }

            public CellInfo(TreeViewColumn column, CellRenderer cellRenderer, ITreeModel model, TreeIter iter)
            {
                this.Column = (column as HierarchyViewColumn)!;
                this.CellRenderer = cellRenderer;
                this.Model = model;
                this.Iter = iter;

                this.HierarchyView = (column.TreeView.GetAncestor(HierarchyView.GType) as HierarchyView)!;
                this.Selected = HierarchyView.IterIsSelected(iter);
                this.Node = (model.GetNode(iter, 0) as PartialNode)!;
            }
        }

        protected void OnRenderingInternal(TreeViewColumn column, CellRenderer cellRenderer, ITreeModel model, TreeIter iter)
        {
            var cellInfo = new CellInfo(column, cellRenderer, model, iter);
            OnRendering(cellInfo);
        }

        protected virtual void OnRendering(CellInfo cellInfo)
        {
            var backgroundColor = OnRequestBackgroundColor(cellInfo);
            textRenderer.Markup = OnRequestText(cellInfo);
            textRenderer.BackgroundGdk = backgroundColor;
            iconRenderer.CellBackgroundGdk = backgroundColor;
            var icon = OnRequestIcon(cellInfo);
            if (icon != null)
            {
                iconRenderer.Pixbuf = icon;
                iconRenderer.Visible = true;
            }
            else
            {
                iconRenderer.Visible = false;
            }
        }

        protected abstract Pixbuf? OnRequestIcon(CellInfo cellInfo);

        protected abstract string OnRequestText(CellInfo cellInfo);

        protected virtual Color OnRequestBackgroundColor(CellInfo cellInfo)
        {
            var selected = cellInfo.Selected;
            Debug.Assert(cellInfo.Node.ParallelNode.Nodes.Count() == 2);
            var differenceType = cellInfo.Node.DifferenceType;
            if (differenceType == Core.DifferenceType.Modify)
            {
                switch (cellInfo.Node.Index)
                {
                    case 0:
                        differenceType = Core.DifferenceType.Remove;
                        break;
                    case 1:
                        differenceType = Core.DifferenceType.Add;
                        break;
                }
            }
            switch (differenceType)
            {
                case DifferenceType.Add:
                    return selected ? new Color(182, 233, 254) : new Color(240, 255, 246);
                case DifferenceType.Remove:
                    return selected ? new Color(197, 221, 252) : new Color(255, 245, 243);
                case DifferenceType.None:
                    return selected ? new Color(207, 233, 255) : new Color(244, 246, 248);
                case DifferenceType.Same:
                    return selected ? new Color(227, 243, 255) : new Color(255, 255, 255);
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}