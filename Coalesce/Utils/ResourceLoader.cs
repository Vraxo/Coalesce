using System.Reflection;

namespace Coalesce.Utils;

public static class ResourceLoader
{
    public static string Get(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"Coalesce.Resources.{resourcePath}";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Could not find embedded resource '{resourceName}'.", resourceName);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}