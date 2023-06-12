using Gtk;
using System.IO;

namespace HierarchyDiff
{
    internal class Program
    {
        private static MainWindow? window;
        
        public static MainWindow Window => window!;

        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                return 1;
            }
            string originPath = args[0];
            string targetPath = args[1];
            if (!File.Exists(originPath) || !File.Exists(targetPath))
            {
                return 1;
            }
            Application.Init();
            window = new MainWindow();
            window.ShowAll();
            window.Initialize();
            window.Load(originPath, targetPath);
            Application.Run();
            return 0;
        }
    }
}