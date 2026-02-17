using System;
using System.IO;
using System.Reflection;

namespace TiaAddin.TiaHost;

public sealed class TiaDependencyGuard
{
    public Assembly EnsureAssemblyLoaded(string path, string requiredAssemblyName)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            throw new FileNotFoundException($"Required dependency not found at '{path}'.");
        }

        var assembly = Assembly.LoadFrom(path);
        if (!string.Equals(assembly.GetName().Name, requiredAssemblyName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Loaded assembly '{assembly.GetName().Name}' does not match expected '{requiredAssemblyName}'.");
        }

        return assembly;
    }
}
