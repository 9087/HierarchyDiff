using System.Windows;
using System.Windows.Controls;

namespace HierarchyDiff.Primitives
{
    public static class TreeViewItemHelper
    {
        #region Gutter

        public static readonly DependencyProperty GutterProperty =
            DependencyProperty.RegisterAttached(
                "Gutter",
                typeof(DataTemplate),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(OnGutterChanged));

        public static DataTemplate GetGutter(TreeViewItem treeViewItem)
        {
            return (DataTemplate)treeViewItem.GetValue(GutterProperty);
        }

        public static void SetGutter(TreeViewItem treeViewItem, DataTemplate value)
        {
            treeViewItem.SetValue(GutterProperty, value);
        }

        private static void OnGutterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Highlighter

        public static readonly DependencyProperty HighlighterProperty =
            DependencyProperty.RegisterAttached(
                "Highlighter",
                typeof(DataTemplate),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(OnHighlighterChanged));

        public static DataTemplate GetHighlighter(TreeViewItem treeViewItem)
        {
            return (DataTemplate)treeViewItem.GetValue(HighlighterProperty);
        }

        public static void SetHighlighter(TreeViewItem treeViewItem, DataTemplate value)
        {
            treeViewItem.SetValue(HighlighterProperty, value);
        }

        private static void OnHighlighterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

    }
}
