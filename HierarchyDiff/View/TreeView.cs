using HierarchyDiff.ViewModel;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HierarchyDiff.View
{
    class TreeView : System.Windows.Controls.TreeView
    {
        public Action<object, ScrollChangedEventArgs>? ScrollChanged { get; set; }

        private ScrollViewer? scrollViewer;

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(TreeView), new PropertyMetadata(ScrollBarVisibility.Auto, OnVerticalScrollBarVisibilityChanged));

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        private static void OnVerticalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            if (treeView.scrollViewer != null)
            {
                treeView.scrollViewer.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
            }
        }

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(TreeView), new PropertyMetadata(ScrollBarVisibility.Auto, OnHorizontalScrollBarVisibilityChanged));

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            if (treeView.scrollViewer != null)
            {
                treeView.scrollViewer.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.BorderThickness = new(0);

            scrollViewer = this.FindDescendant<ScrollViewer>();
            scrollViewer.ScrollChanged += OnTreeScrollChanged;

            scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
            scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
        }

        private void OnTreeScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollChanged?.Invoke(sender, e);
        }

        public void ScrollToHorizontalOffset(double offset)
        {
            scrollViewer?.ScrollToHorizontalOffset(offset);
        }

        public void ScrollToVerticalOffset(double offset)
        {
            scrollViewer?.ScrollToVerticalOffset(offset);
        }

        public static readonly DependencyProperty IsSelectingProperty =
            DependencyProperty.Register(
                nameof(IsSelecting),
                typeof(bool),
                typeof(TreeView),
                new PropertyMetadata(OnIsSelectingChanged));

        public bool IsSelecting
        {
            get => (bool)GetValue(IsSelectingProperty);
            set => SetValue(IsSelectingProperty, value);
        }

        private bool isPreviewMouseLeftButtonDown = false;

        private static void OnIsSelectingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TreeView).isPreviewMouseLeftButtonDown = false;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            RaiseTreeViewItemSelected(e, false);
            base.OnPreviewMouseLeftButtonDown(e);
            isPreviewMouseLeftButtonDown = true;
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);
            if (isPreviewMouseLeftButtonDown)
            {
                IsSelecting = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsSelecting)
            {
                RaiseTreeViewItemSelected(e, true);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            if (IsSelecting)
            {
                RaiseTreeViewItemSelected(e, true);
            }
            IsSelecting = false;
        }

        private void RaiseTreeViewItemSelected(System.Windows.Input.MouseEventArgs e, bool selected)
        {
            var hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hitTestResult == null)
            {
                return;
            }
            DependencyObject clickedObject = hitTestResult.VisualHit;
            while (clickedObject != null && !(clickedObject is TreeViewItem))
            {
                clickedObject = VisualTreeHelper.GetParent(clickedObject);
            }
            TreeViewItem? treeViewItem = clickedObject as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = selected;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsSelecting = false;
        }
    }
}
