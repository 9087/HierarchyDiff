using CommunityToolkit.Mvvm.Input;
using HierarchyDiff.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace HierarchyDiff.ViewModel
{
    internal class ParallelTreeViewModel
    {
        public ComparisonViewModel Outer { get; private set; }

        public ComparisonViewModel Comparison => Outer;

        public SlotIndex Index { get; private set; }

        public ObservableCollection<ParallelTreeNodeViewModel> ItemsSource { get; private set; }

        public string FilePath => Comparison?.Documents[(int)Index].FilePath ?? string.Empty;

#pragma warning disable CS8618
        public ParallelTreeViewModel() { }
#pragma warning restore CS8618

        private Dictionary<TreeNode<Tuple<object?, object?>>, ParallelTreeNodeViewModel> nodes = new();

        public ICommand UpdateHighlightRangeCommand { get; set; }

        public ParallelTreeViewModel(ComparisonViewModel outer, SlotIndex index)
        {
            this.Outer = outer;
            this.Index = index;
            this.ItemsSource = new() { new(this, Comparison.Model.GetRoot(), index) };

            UpdateHighlightRangeCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>((args) => { Comparison.UpdateHighlightRange(args!); });
        }

        internal void RegisterNode(ParallelTreeNodeViewModel parallelTreeNodeViewModel)
        {
            nodes[parallelTreeNodeViewModel.Node] = parallelTreeNodeViewModel;
        }

        internal ParallelTreeNodeViewModel? GetNode(ParallelTreeNode node)
        {
            return nodes.GetValueOrDefault(node);
        }

        internal void UnregisterNode(ParallelTreeNodeViewModel parallelTreeNodeViewModel)
        {
            var key = parallelTreeNodeViewModel.Node;
            Debug.Assert(nodes.ContainsKey(key));
            Debug.Assert(nodes[key] == parallelTreeNodeViewModel);
            nodes.Remove(key);
        }
    }
}
