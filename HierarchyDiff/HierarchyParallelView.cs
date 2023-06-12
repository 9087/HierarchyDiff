using Gdk;
using Gtk;
using HierarchyDiff.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace HierarchyDiff
{
    internal class PathButton : Button
    {
        private string path = string.Empty;

        public PathButton()
        {
            this.Label = path;
            this.StyleContext.AddClass("-hierarchy-diff-file-path-header-title");
            (this.Child as Label)!.Ellipsize = Pango.EllipsizeMode.End;
        }

        public void SetPath(string path)
        {
            this.path = path;
            (this.Child as Label)!.Markup = path;
        }

        protected override bool OnButtonReleaseEvent(EventButton evnt)
        {
            if (evnt.State.HasFlag(ModifierType.Button3Mask))
            {
                var menu = new Gtk.Menu();

                var copyFullPathMenuItem = new MenuItem("Copy Full Path");
                copyFullPathMenuItem.Activated += (s, e) =>
                {
                    Gtk.Clipboard.Get(Gdk.Selection.Clipboard).Text = this.path;
                };
                menu.Append(copyFullPathMenuItem);

                var openContainingFolderMenuItem = new MenuItem("Open Containing Folder");
                openContainingFolderMenuItem.Activated += (s, e) =>
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = "/e,/select," + this.path;
                    System.Diagnostics.Process.Start(psi);
                };
                menu.Append(openContainingFolderMenuItem);

                menu.ShowAll();
                menu.Popup();
            }
            return base.OnButtonReleaseEvent(evnt);
        }
    }

    public class HierarchyParallelView : Gtk.Bin
    {
        private uint count;
        private List<HierarchyView> hierarchyViews;
        private List<PathButton> pathButtons = new();

        public HierarchyView this[int index] => hierarchyViews[index];

        private HierarchyParallelModel? model;

        public HierarchyParallelModel? Model
        {
            get => model;

            set
            {
                if (value == null || value.Count() != count)
                {
                    throw new System.ArgumentException();
                }    
                model = value;
                if (model != null)
                {
                    hierarchyViews
                        .Zip(model)
                        .ToList()
                        .ForEach(p => p.First.Model = p.Second);
                }
            }
        }

        ScrolledWindow scrolledWindow;
        HBox viewBox;

        public HierarchyParallelView(uint count)
        {
            this.count = count;
            hierarchyViews = Enumerable
                .Repeat<object?>(null, (int)count)
                .Select(_ => new HierarchyView())
                .ToList();
            foreach (var hierarchyView in hierarchyViews)
            {
                hierarchyView.ColumnWidthChanged += OnHierarchyViewColumnWidthChanged;
            }
            var box = new VBox();
            this.Add(box);
            
            var pathButtonBox = new HBox();
            var titleSizeGroup = new SizeGroup(SizeGroupMode.Horizontal);
            pathButtonBox.HeightRequest = 32;
            pathButtonBox.StyleContext.AddClass("-hierarchy-diff-file-path-header");
            box.PackStart(pathButtonBox, false, true, 0);
            for (int i = 0; i < count; i++)
            {
                if (i != 0)
                {
                    var separator = new Gtk.VSeparator { WidthRequest = 5 };
                    separator.StyleContext.AddClass("-hierarchy-diff-file-path-header-separator");
                    pathButtonBox.PackStart(separator, false, true, 0);
                }
                var pathButton = new PathButton();
                pathButtonBox.PackStart(pathButton, true, true, 0);
                titleSizeGroup.AddWidget(pathButton);
                pathButtons.Add(pathButton);
            }
            scrolledWindow = new ScrolledWindow();
            scrolledWindow.HscrollbarPolicy = PolicyType.Never;
            box.PackStart(scrolledWindow, true, true, 0);

            viewBox = new HBox();
            var viewSizeGroup = new SizeGroup(SizeGroupMode.Horizontal);
            scrolledWindow.Add(viewBox);
            for (int i = 0; i < hierarchyViews.Count; i++)
            {
                if (i != 0)
                {
                    var separator = new Gtk.VSeparator { WidthRequest = 5 };
                    separator.StyleContext.AddClass("-hierarchy-diff-hierarchy-view-separator");
                    viewBox.PackStart(separator, false, true, 0);
                }
                var hierarchyView = hierarchyViews[i];
                viewSizeGroup.AddWidget(hierarchyView);
                viewBox.PackStart(hierarchyView, true, true, 0);
                hierarchyView.NodeCollapsed += OnNodeCollapsed;
                hierarchyView.NodeExpanded += OnNodeExpanded;
                hierarchyView.NodeSelectionChanged += OnNodeSelectionChanged;
            }
            Model = new HierarchyParallelModel(count);
        }

        private void OnHierarchyViewColumnWidthChanged(int index, int width)
        {
            foreach (var hierarchyView in hierarchyViews)
            {
                hierarchyView.SetColumnWidth(index, width);
            }
        }

        private void OnNodeExpanded(HierarchyDiff.Core.Node node)
        {
            if (node is PartialNode partialNode)
            {
                ExpandParallelNode(partialNode.ParallelNode);
            }
        }

        private void OnNodeCollapsed(HierarchyDiff.Core.Node node)
        {
            if (node is PartialNode partialNode)
            {
                CollapseParallelNode(partialNode.ParallelNode);
            }
        }

        private void OnNodeSelectionChanged(HashSet<HierarchyDiff.Core.Node> nodes)
        {
            SelectParallelNodes(nodes.Select(x => x as PartialNode).Where(x => x != null).Select(x => x!.ParallelNode).ToHashSet());
        }

        public void ExpandParallelNode(ParallelNode parallelNode)
        {
            for (int i = 0; i < this.count; i++)
            {
                var model = Model![i];
                var view = hierarchyViews[i];
                var iter = model.FindPartialNode(parallelNode);
                view.ExpandRow(model.GetPath(iter!.Value), false);
            }
        }

        public void CollapseParallelNode(ParallelNode parallelNode)
        {
            for (int i = 0; i < this.count; i++)
            {
                var model = Model![i];
                var view = hierarchyViews[i];
                var iter = model.FindPartialNode(parallelNode);
                view.CollapseRow(model.GetPath(iter!.Value));
            }
        }

        public void SelectParallelNodes(HashSet<ParallelNode> parallelNodes)
        {
            for (int i = 0; i < this.count; i++)
            {
                var model = Model![i];
                var view = hierarchyViews[i];
                view.SelectPaths(parallelNodes.Select(x => model.GetPath(model.FindPartialNode(x)!.Value)).ToHashSet());
            }
        }

        public void LoadFromCompareSession(ComparingSession session)
        {
            pathButtons[0].SetPath($"{session.OriginPath}");
            pathButtons[1].SetPath($"{session.TargetPath}");
            this.Model![0].LoadFromDocument(session.ParallelOriginDocument);
            this.Model![1].LoadFromDocument(session.ParallelTargetDocument);
            this[0].ExpandAll();
            this[1].ExpandAll();
        }
    }
}
