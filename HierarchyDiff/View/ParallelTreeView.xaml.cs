using HierarchyDiff.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace HierarchyDiff.View
{
    public partial class ParallelTreeView : UserControl
    {
        public Action<object, ScrollChangedEventArgs>? ScrollChanged { get; set; }

        #region Property: VerticalScrollBarVisibility

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ParallelTreeView), new PropertyMetadata(ScrollBarVisibility.Auto, OnVerticalScrollBarVisibilityChanged));

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        private static void OnVerticalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var parallelTreeView = (ParallelTreeView)d;
            parallelTreeView.Tree.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }

        #endregion

        #region Property: HorizontalScrollBarVisibility

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ParallelTreeView), new PropertyMetadata(ScrollBarVisibility.Auto, OnHorizontalScrollBarVisibilityChanged));

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var parallelTreeView = (ParallelTreeView)d;
            parallelTreeView.Tree.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }

        #endregion

        #region Property: HighlightBrush

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(ParallelTreeView), new PropertyMetadata(null, OnHighlightBrushChanged));

        public Brush HighlightBrush
        {
            get => (Brush)GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        private static void OnHighlightBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Property: PlaceholderBrush

        public static readonly DependencyProperty LowlightBrushProperty = DependencyProperty.Register(nameof(PlaceholderBrush), typeof(Brush), typeof(ParallelTreeView), new PropertyMetadata(null, OnLowlightBrushChanged));

        public Brush PlaceholderBrush
        {
            get => (Brush)GetValue(LowlightBrushProperty);
            set => SetValue(LowlightBrushProperty, value);
        }

        private static void OnLowlightBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        private bool isTreeMouseLeftButtonDown = false;

        public ParallelTreeView()
        {
            InitializeComponent();

            Tree.ScrollChanged += OnTreeScrollChanged;
        }

        private void OnTreeScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollChanged?.Invoke(sender, e);
        }

        public void ScrollToHorizontalOffset(double offset)
        {
            Tree.ScrollToHorizontalOffset(offset);
        }

        public void ScrollToVerticalOffset(double offset)
        {
            Tree.ScrollToVerticalOffset(offset);
        }
    }
}
