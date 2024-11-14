using HierarchyDiff.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace HierarchyDiff.View
{
    public partial class ComparisonView : UserControl
    {
        private List<ParallelTreeView> parallelTreeViews; 

        public ComparisonView()
        {
            InitializeComponent();

            this.parallelTreeViews = new() { this.TreeView0, this.TreeView1 };
            foreach (var parallelTreeView in parallelTreeViews)
            {
                parallelTreeView.ScrollChanged += OnTreeScrollChanged;
            }

            this.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                var selectedNode = this.TreeView1.Tree.SelectedItem as ParallelTreeNodeViewModel;
                if (selectedNode != null)
                {
                    selectedNode.StartEditing();
                    e.Handled = true;
                }
            }
        }

        private void OnTreeScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            foreach (var parallelTreeView in parallelTreeViews)
            {
                parallelTreeView.ScrollToHorizontalOffset(e.HorizontalOffset);
                parallelTreeView.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }
    }
}
