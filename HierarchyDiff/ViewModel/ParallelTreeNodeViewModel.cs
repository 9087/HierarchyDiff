using CommunityToolkit.Mvvm.Input;
using HierarchyDiff.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;

namespace HierarchyDiff.ViewModel
{
    internal enum Operation
    {
        None,
        Add,
        Delete,
        Modify,
    }

    internal class ParallelTreeNodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            InvokePropertyChanged(this, e);
        }

        protected void InvokePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public ParallelTreeViewModel Outer { get; private set; }

        public ParallelTreeViewModel ParallelTree => Outer;

        public ParallelTreeNode Node { get; private set; }

        public SlotIndex Index { get; private set; }

        public ObservableCollection<ParallelTreeNodeViewModel> Children { get; private set; } = new();

        public Core.FileFormat FileFormat => this.ParallelTree.Comparison?.FileFormat!;

        public ICommand StopEditingCommand { get; }

        public ICommand KeyDownCommand { get; }

        public ParallelTreeNodeViewModel(ParallelTreeViewModel outer, ParallelTreeNode node, SlotIndex index)
        {
            this.Outer = outer;
            this.Node = node;
            this.Index = index;
            this.ParallelTree.RegisterNode(this);
            BuildChildren();

            StopEditingCommand = new RelayCommand(StopEditing);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(OnKeyDown);

            Node.ValueChanged += OnNodeValueChanged;
        }

        void OnKeyDown(KeyEventArgs? keyEventArgs)
        {
            if (keyEventArgs == null)
            {
                return;
            }
            switch (keyEventArgs.Key)
            {
                case Key.Enter:
                    keyEventArgs.Handled = true;
                    StopEditing();
                    break;
            }
        }

        ~ParallelTreeNodeViewModel()
        {
            this.ParallelTree.UnregisterNode(this);
        }

        private void BuildChildren()
        {
            for (int index = 0; index < this.Node.GetChildCount(); index++)
            {
                var child = this.Node.GetChildChecked<ParallelTreeNode>(index);
                Debug.Assert(child != null);
                this.Children.Add(new(Outer, child, Index));
            }
            Style = Node.GetStyle(Index, Style);
        }

        public ParallelTreeNodeViewModel? GetParent()
        {
            var parent = this.Node.Parent as ParallelTreeNode;
            if (parent == null)
            {
                return null;
            }
            return this.ParallelTree.GetNode(parent);
        }

        public bool IsPredecessorOf(ParallelTreeNodeViewModel other)
        {
            if (other == this)
            {
                return false;
            }
            if (other.ParallelTree != this.ParallelTree)
            {
                return false;
            }
            return this.Node.IsPredecessorOf(other.Node);
        }

        internal ParallelTreeNodeViewModel? GetParallelNode(uint index)
        {
            var treeViewModel = this.ParallelTree.Comparison.GetTree(index);
            return treeViewModel?.GetNode(Node);
        }

        private void ForEachParallelNode(Action<ParallelTreeNodeViewModel> callback)
        {
            ParallelTreeNodeViewModel? parallelNode = null;
            for (uint index = 0; (parallelNode = GetParallelNode(index)) != null; index++)
            {
                if (parallelNode == this) continue;
                callback(parallelNode);
            }
        }

        public string Name => Node.GetName(Index);

        public string Value => Node.GetValue(Index);

        private void OnNodeValueChanged(SlotIndex index, string value)
        {
            InvokePropertyChanged(this, new(nameof(Value)));
            for (uint i = 0; ;i++)
            {
                var parallelNode = GetParallelNode(i);
                if (parallelNode == null)
                {
                    break;
                }
                parallelNode.InvokePropertyChanged(new(nameof(Operation)));
            }
        }

        public bool IsNull => Node.IsNull(Index);

        public Operation Operation
        {
            get
            {
                if (Index == SlotIndex.Source && !Node.IsNull(SlotIndex.Source) && Node.IsNull(SlotIndex.Target))
                {
                    return Operation.Delete;
                }
                if (Index == SlotIndex.Target && Node.IsNull(SlotIndex.Source) && !Node.IsNull(SlotIndex.Target))
                {
                    return Operation.Add;
                }
                if (!Node.IsNull(SlotIndex.Source) && !Node.IsNull(SlotIndex.Target) && Node.GetName(SlotIndex.Source) != Node.GetName(SlotIndex.Target))
                {
                    if (Index == SlotIndex.Source) return Operation.Delete;
                    if (Index == SlotIndex.Target) return Operation.Add;
                }
                if (!Node.IsNull(SlotIndex.Source) && !Node.IsNull(SlotIndex.Target) && Node.GetValue(SlotIndex.Source) != Node.GetValue(SlotIndex.Target))
                {
                    return Operation.Modify;
                }
                return Operation.None;
            }
        }

        #region Property: IsSelected

        private bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value) return;
                isSelected = value;
                InvokePropertyChanged(this, new(nameof(IsSelected)));
                ForEachParallelNode((parallelNode) =>
                {
                    parallelNode.IsSelected = value;
                });
            }
        }

        #endregion

        #region Property: IsExpanded

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                InvokePropertyChanged(this, new(nameof(IsExpanded)));
                ForEachParallelNode((parallelNode) => { parallelNode.IsExpanded = value; });
            }
        }

        #endregion

        #region Property: IsEditing

        private static ParallelTreeNodeViewModel? editing = null;

        public bool IsEditing
        {
            get => editing == this;
            set
            {
                if (value == IsEditing)
                {
                    return;
                }
                var old = editing;
                if (value)
                {
                    editing = this;
                }
                else
                {
                    editing = null;
                }
                old?.InvokePropertyChanged(old, new(nameof(IsEditing)));
                this.InvokePropertyChanged(this, new(nameof(IsEditing)));
            }
        }

        public string EditingValue { get; set; } = string.Empty;

        public void StartEditing()
        {
            IsEditing = true;
            EditingValue = Value;
            this.ParallelTree.Comparison.HighlightRange.SetInitial(this.Node);
            this.InvokePropertyChanged(this, new(nameof(EditingValue)));
        }

        private void StopEditing()
        {
            IsEditing = false;
            if (EditingValue == Value)
            {
                return;
            }
            this.Node.Undoable_ChangeValue(Index, EditingValue);
        }

        #endregion

        #region Property: IsHighlighted

        public bool IsFirstHighlighted => this.ParallelTree.Comparison.HighlightRange.First == this.Node;

        public bool IsLastHighlighted => this.ParallelTree.Comparison.HighlightRange.Last == this.Node;

        public bool IsHighlighted => this.ParallelTree.Comparison.HighlightRange.Contains(this.Node);

        public void RaiseIsHighlightedPropertyChanged()
        {
            this.InvokePropertyChanged(this, new(nameof(IsHighlighted)));
            this.InvokePropertyChanged(this, new(nameof(IsFirstHighlighted)));
            this.InvokePropertyChanged(this, new(nameof(IsLastHighlighted)));
        }

        #endregion

        public bool CollapseUnimportantDescendants()
        {
            bool important = Operation != Operation.None;
            foreach (var child in Children)
            {
                important |= !child.CollapseUnimportantDescendants();
            }
            if (!important)
            {
                IsExpanded = false;
            }
            return !important;
        }

        #region Item Style

        public TreeNodeStyle Style { get; private set; } = new();

        public FontFamily FontFamily => new FontFamily(Style.FontFamily ?? "Consolas");

        public double FontSize => Style.FontSize ?? 12;

        public double LineHeight => Style.LineHeight ?? 16;

        #endregion
    }
}
