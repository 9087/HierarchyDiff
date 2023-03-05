namespace HierarchyDiff.Core
{
    public class Image
    {
        public string? Path { get; private set; }

        public object? Data { get; set; }

        public int Width { get; private set; } = 0;

        public int Height { get; private set; } = 0;

        public Image(string? path)
        {
            this.Path = path;
        }

        public Image(string? path, int width, int height)
        {
            this.Path = path;
            this.Width = width;
            this.Height = height;
        }

        public static Image Empty = new Image(null);
    }

    public static class Icons
    {
        public static Image Property = new Image("Resources://Property.png", 12, 12);
    }
}
