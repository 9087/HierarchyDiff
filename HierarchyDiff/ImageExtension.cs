using Gdk;
using Gtk;
using HierarchyDiff.Core;
using System.IO;
using System.Reflection;

namespace HierarchyDiff
{
    public static class ImageExtension
    {
        public static Pixbuf? GetPixbuf(this Core.Image image)
        {
            if (image.Path == null)
            {
                return null;
            }
            if (image.Data == null)
            {
                if (image.Path.StartsWith("Resources://", System.StringComparison.OrdinalIgnoreCase))
                {
                    var builtinPath = $"HierarchyDiff.Resources.{image.Path.Substring("Resources://".Length)}";
                    using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(builtinPath))
                    {
                        if (stream == null)
                        {
                            throw new System.Exception();
                        }
                        if (image.Width * image.Height == 0)
                        {
                            image.Data = new Pixbuf(stream);
                        }
                        else
                        {
                            image.Data = new Pixbuf(stream, image.Width, image.Height);
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            return image.Data as Pixbuf;
        }
    }
}
