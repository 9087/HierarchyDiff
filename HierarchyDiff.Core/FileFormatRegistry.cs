using System.Reflection;

namespace HierarchyDiff.Core
{
    internal class FileFormatRegistry
    {
        private static FileFormatRegistry? singleton;

        public static FileFormatRegistry Instance => singleton ??= new();

        private static Dictionary<string, Type> fileFormatTypes = new();

        private Dictionary<string, FileFormat> fileFormatCache = new();

        static FileFormatRegistry()
        {
            FileFormatRegistry.Instance.RegisterTypes(new(AppDomain.CurrentDomain.BaseDirectory));
        }

        public void RegisterTypes(Uri searchPath)
        {
            foreach (var assemblyPath in Directory.GetFiles(searchPath.AbsolutePath, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                if (assembly == null)
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(FileFormat).IsAssignableFrom(type))
                    {
                        continue;
                    }
                    RegisterType(type);
                }
            }
        }

        public bool RegisterType(Type type)
        {
            if (!typeof(FileFormat).IsAssignableFrom(type))
            {
                return false;
            }
            if (type.GetConstructor(new Type[0]) == null)
            {
                return false;
            }
            var extensionName = type.Name.ToLower();
            if (!fileFormatTypes.ContainsKey(extensionName))
            {
                fileFormatTypes.Add(extensionName, type);
            }
            else
            {
                fileFormatTypes[extensionName] = type;
            }
            return true;
        }

        public FileFormat? GetOrCreateFileFormat(string extensionName)
        {
            if (extensionName.StartsWith('.'))
            {
                throw new ArgumentException("The extension name cannot start with a dot.");
            }
            extensionName = extensionName.ToLower();
            if (fileFormatCache.TryGetValue(extensionName, out var fileFormat))
            {
                return fileFormat;
            }
            if (!fileFormatTypes.TryGetValue(extensionName, out var type))
            {
                return null;
            }
            fileFormat = Activator.CreateInstance(type) as FileFormat;
            fileFormatCache[extensionName] = fileFormat!;
            return fileFormat;
        }
    }
}
