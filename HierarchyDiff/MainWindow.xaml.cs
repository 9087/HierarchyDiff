using Fluent;
using HierarchyDiff.ViewModel;
using System.Windows;

namespace HierarchyDiff
{
    public partial class MainWindow : Window, Fluent.IRibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            if (App.Current.Arguments.Length >= 2)
            {
                var a = HierarchyDiff.Core.Document.Load(App.Current.Arguments[0]);
                var b = HierarchyDiff.Core.Document.Load(App.Current.Arguments[1]);
                if (a != null && b != null)
                {
                    var comparison = HierarchyDiff.Core.Comparison.Create(a!, b!);
                    this.DataContext = new ComparisonViewModel(comparison);
                    return;
                }
            }
            this.DataContext = new ComparisonViewModel();
        }

        public RibbonTitleBar? TitleBar => null;
    }
}