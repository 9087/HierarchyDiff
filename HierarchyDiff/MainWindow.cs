using GLib;
using Gtk;
using HierarchyDiff.Core;
using System;
using System.IO;
using System.Reflection;

namespace HierarchyDiff
{
    internal class MainWindow : Window
    {
        private readonly HierarchyParallelView parallelView = new(2);

        public bool Initialized { get; private set; } = false;

        public MainWindow() : base("Hierarchy Diff")
        {
            using (Stream? styleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HierarchyDiff.Resources.Style.css"))
            if (styleStream != null)
            using (var reader = new StreamReader(styleStream!))
            {
                var css = new CssProvider();
                css.LoadFromData(reader.ReadToEnd());
                StyleContext.AddProviderForScreen(this.Screen, css, Gtk.StyleProviderPriority.User);
            };
            SetDefaultSize(1024, 768);
            DeleteEvent += OnWindowDeleted;
            Add(parallelView);
        }

        public void Initialize()
        {
            this.Initialized = true;
        }

        public bool Load(string originPath, string targetPath)
        {
            var originExtensionName = System.IO.Path.GetExtension(originPath);
            var targetExtensionName = System.IO.Path.GetExtension(targetPath);
            if (originExtensionName != targetExtensionName)
            {
                throw new ArgumentException();
            }
            if (!File.Exists(originPath) || !File.Exists(targetPath))
            {
                return false;
            }
            var session = new ComparingSession(originPath, targetPath);
            parallelView.LoadFromCompareSession(session);
            return true;
        }

        private void OnWindowDeleted(object o, DeleteEventArgs args)
        {
            Gtk.Application.Quit();
        }
    }
}
