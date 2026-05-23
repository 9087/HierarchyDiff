using Fluent;
using HierarchyDiff.Core;
using HierarchyDiff.ViewModel;
using Microsoft.Win32;
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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var filter = FileFormatRegistry.Instance.GetFileFilter();

            var sourceDialog = new OpenFileDialog
            {
                Title = "Select Source document",
                Filter = filter
            };
            if (sourceDialog.ShowDialog() != true)
            {
                return;
            }

            var targetDialog = new OpenFileDialog
            {
                Title = "Select Target document",
                Filter = filter
            };
            if (targetDialog.ShowDialog() != true)
            {
                return;
            }

            var a = Document.Load(sourceDialog.FileName);
            var b = Document.Load(targetDialog.FileName);
            if (a != null && b != null)
            {
                var comparison = Comparison.Create(a, b);
                this.DataContext = new ComparisonViewModel(comparison);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ComparisonViewModel vm)
            {
                vm.Model.FileFormat.Save(vm.TargetDocument.Object!, vm.TargetDocument.FilePath);
            }
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ComparisonViewModel vm)
            {
                var source = Document.Load(vm.SourceDocument.FilePath);
                var target = Document.Load(vm.TargetDocument.FilePath);
                if (source != null && target != null)
                {
                    var comparison = Comparison.Create(source, target);
                    this.DataContext = new ComparisonViewModel(comparison);
                }
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ComparisonViewModel vm)
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save target document as",
                    Filter = FileFormatRegistry.Instance.GetFileFilter(),
                    FileName = System.IO.Path.GetFileName(vm.TargetDocument.FilePath)
                };
                if (dialog.ShowDialog() == true)
                {
                    vm.Model.FileFormat.Save(vm.TargetDocument.Object!, dialog.FileName);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}