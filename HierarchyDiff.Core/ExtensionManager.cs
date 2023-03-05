using System.Diagnostics;
using System.Reflection;

namespace HierarchyDiff.Core
{
    public static class ExtensionManager
    {
        private static string ExecutableDirectoryPath => AppDomain.CurrentDomain.BaseDirectory;

        private static Dictionary<string, ISerializer> serializers = new();

        public static Dictionary<string, ISerializer> Serializers => serializers;

        private static bool RegisterExtensionInstance<T>(Dictionary<string, T> cache, Type type) where T : class
        {
            var formatAttribute = type.GetCustomAttribute<FormatAttribute>();
            if (formatAttribute == null)
            {
                return false;
            }
            var extensionName = formatAttribute.ExtensionName.ToLower();
            if (cache.ContainsKey(extensionName))
            {
                return false;
            }
            var instance = Activator.CreateInstance(type) as T;
            Debug.Assert(instance != null);
            cache.Add(extensionName, instance);
            return true;
        }

        static ExtensionManager()
        {
            foreach (var path in Directory.GetFiles(ExecutableDirectoryPath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                var assembly = Assembly.LoadFile(path);
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ISerializer).IsAssignableFrom(type))
                    {
                        if (RegisterExtensionInstance(serializers, type))
                        {
                            Debug.WriteLine($"Serializer {type.FullName} is registed.");
                        }
                        else
                        {
                            Debug.WriteLine($"Error: Cannot register serializer {type.FullName}.");
                        }
                    }
                }
            }
        }

        public static ISerializer? GetSerializerByExtensionName(string extensionName)
        {
            extensionName = extensionName.ToLower();
            if (!serializers.ContainsKey(extensionName))
            {
                return null;
            }
            return serializers[extensionName];
        }
    }
}
