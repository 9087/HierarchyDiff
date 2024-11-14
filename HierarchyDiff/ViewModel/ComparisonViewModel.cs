using CommunityToolkit.Mvvm.Input;
using HierarchyDiff.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace HierarchyDiff.ViewModel
{
    internal class ComparisonViewModel
    {
        public Comparison Model { get; private set; }

        public List<ParallelTreeViewModel> Trees { get; private set; } = new();

        public List<Document> Documents => Model.Documents;

        public ParallelTreeViewModel? SourceTree => Trees.ElementAtOrDefault((int)SlotIndex.Source);

        public ParallelTreeViewModel? TargetTree => Trees.ElementAtOrDefault((int)SlotIndex.Target);

        public Document SourceDocument => Documents[(int)SlotIndex.Source];

        public Document TargetDocument => Documents[(int)SlotIndex.Target];

        public HierarchyDiff.Core.FileFormat FileFormat => this.Model.FileFormat;

        public NodeRange HighlightRange { get; private set; }

        public bool IsSelecting
        {
            get;
            set;
        }

        public ICommand SaveCommand { get; }

#pragma warning disable CS8618
        public ComparisonViewModel() { }
#pragma warning restore CS8618

        public ParallelTreeViewModel? GetTree(uint index)
        {
            if (index >= Trees.Count)
            {
                return null;
            }
            return Trees[(int)index];
        }

        public ComparisonViewModel(Comparison comparison)
        {
            this.Model = comparison;
            this.Trees.Add(new ParallelTreeViewModel(this, SlotIndex.Source));
            this.Trees.Add(new ParallelTreeViewModel(this, SlotIndex.Target));
            this.SourceTree!.ItemsSource.First()?.CollapseUnimportantDescendants();
            SaveCommand = new RelayCommand(() =>
            {
                SaveTargetDocument();
            });
            this.HighlightRange = new(this);
        }

        private void SaveTargetDocument()
        {
            this.Model.FileFormat.Save(TargetDocument.Object!, TargetDocument.FilePath);
        }

        public class NodeRange
        {
            private ComparisonViewModel comparisonViewModel;

            private ParallelTreeNode? initial;

            private ParallelTreeNode? current;

            public ParallelTreeNode? Initial => initial;

            public ParallelTreeNode? First { get; private set; }

            public ParallelTreeNode? Last { get; private set; }

            public NodeRange Clone()
            {
                return new NodeRange(comparisonViewModel) { initial = initial, current = current, First = First, Last = Last };
            }

            public NodeRange(ComparisonViewModel comparisonViewModel)
            {
                this.comparisonViewModel = comparisonViewModel;
            }

            private void Update(ParallelTreeNode? selected, bool append)
            {
                if (!append || First == null)
                {
                    initial = current = selected;
                }
                else
                {
                    if (selected != null)
                    {
                        current = selected;
                    }
                    else
                    {
                        current = initial;
                    }
                }
                Arrange();
            }

            public void Update(ParallelTreeNode? initial, ParallelTreeNode? current)
            {
                var previous = Clone();
                Update(initial, false);
                Update(current, true);
                previous.RaisePropertyChanged();
                RaisePropertyChanged();
            }

            public void SetInitial(ParallelTreeNode? initial)
            {
                var previous = Clone();
                Update(initial, false);
                previous.RaisePropertyChanged();
                RaisePropertyChanged();
            }

            public void SetCurrent(ParallelTreeNode? current)
            {
                var previous = Clone();
                Update(current, true);
                previous.RaisePropertyChanged();
                RaisePropertyChanged();
            }

            private void RaisePropertyChanged()
            {
                Traverse(node =>
                {
                    comparisonViewModel.Trees.ForEach(tree => { tree!.GetNode(node)!.RaiseIsHighlightedPropertyChanged(); });
                },
                TraverseMethod.Presentation);
            }

            public bool Contains(ParallelTreeNode node)
            {
                Debug.Assert(node != null);
                if (First == node) { return true; }
                if (Last == node) { return true; }
                if (First == Last) { return false; }
                if (First == null || Last == null) { return false; }
                return First.IsPredecessorOf(node) && node.IsPredecessorOf(Last);
            }

            public enum TraverseMethod
            {
                Presentation,
                Final,
            }

            public void Traverse(Action<ParallelTreeNode> callback, TraverseMethod traverseMethod)
            {
                if (First == null || Last == null)
                {
                    return;
                }
                var current = First;
                while (current != null && current.Traverse((node) =>
                {
                    callback(node);
                    switch (traverseMethod)
                    {
                        case TraverseMethod.Presentation:
                            return node != Last;
                        case TraverseMethod.Final:
                            var sourceNodeViewModel = this.comparisonViewModel.SourceTree!.GetNode(node);
                            return sourceNodeViewModel!.IsExpanded ? node != Last : true;
                        default:
                            throw new NotImplementedException();
                    }
                }))
                {
                    while (current != null)
                    {
                        var nextSibling = current!.GetNextSibling();
                        if (nextSibling != null)
                        {
                            current = nextSibling;
                            break;
                        }
                        current = current.Parent as ParallelTreeNode;
                    }
                }
            }

            private void Arrange()
            {
                First = initial;
                Last = current;
                if (First == Last)
                {
                    return;
                }
                if (First == null) { Last = First; return; }
                if (Last == null) { First = Last; return; }
                if (!First.IsPredecessorOf(Last))
                {
                    var tempory = First;
                    First = Last;
                    Last = tempory;
                }
            }
        }

        public void UpdateHighlightRange(RoutedPropertyChangedEventArgs<object> args)
        {
            var oldSelectedNode = (args.OldValue as ParallelTreeNodeViewModel)?.Node;
            var selectedNode = (args.NewValue as ParallelTreeNodeViewModel)?.Node;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) || this.IsSelecting)
            {
                this.HighlightRange.SetCurrent(selectedNode);
            }
            else
            {
                this.HighlightRange.SetInitial(selectedNode);
            }
        }
    }
}
