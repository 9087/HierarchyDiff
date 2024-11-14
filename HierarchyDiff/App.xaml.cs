using System.Windows;

namespace HierarchyDiff
{
    public partial class App : Application
    {
        public static new App Current => (Application.Current as App)!;

        public string[] Arguments { get; private set; } = new string[0];

        protected override void OnStartup(StartupEventArgs e)
        {
            Arguments = e.Args;
            base.OnStartup(e);
        }
    }
}
