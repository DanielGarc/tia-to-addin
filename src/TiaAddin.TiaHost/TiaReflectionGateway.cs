using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TiaAddin.Core;

namespace TiaAddin.TiaHost;

public sealed class TiaReflectionGateway : IProjectGateway
{
    private readonly object _project;
    private readonly Dictionary<string, object> _objectIndex = new(StringComparer.OrdinalIgnoreCase);

    public TiaReflectionGateway(object engineeringProject)
    {
        _project = engineeringProject ?? throw new ArgumentNullException(nameof(engineeringProject));
    }

    public IReadOnlyList<AutomationItem> GetSelectedItems()
    {
        _objectIndex.Clear();
        var result = new List<AutomationItem>();
        Traverse(_project, result);
        return result;
    }

    public void ApplyChanges(ChangeSet changeSet)
    {
        foreach (var action in changeSet.Actions)
        {
            if (string.Equals(action.Field, "CreateTechnologicalObject", StringComparison.OrdinalIgnoreCase))
            {
                CreateTechnologicalObject(action.NewValue);
                continue;
            }

            if (!_objectIndex.TryGetValue(action.ItemId, out var target))
            {
                continue;
            }

            if (string.Equals(action.Field, "Name", StringComparison.OrdinalIgnoreCase))
            {
                ReflectionSet(target, "Name", action.NewValue);
                continue;
            }

            if (action.Field.StartsWith("Setting:", StringComparison.OrdinalIgnoreCase))
            {
                var key = action.Field.Substring("Setting:".Length);
                SetSetting(target, key, action.NewValue);
            }
        }
    }

    private void Traverse(object node, IList<AutomationItem> buffer)
    {
        if (node is null)
        {
            return;
        }

        if (IsTechnologicalObject(node))
        {
            var id = ReflectionGet(node, "Id")?.ToString() ?? Guid.NewGuid().ToString("N");
            var name = ReflectionGet(node, "Name")?.ToString() ?? string.Empty;
            var path = ReflectionGet(node, "Path")?.ToString() ?? "TechnologicalObjects";
            var settings = ReadSettings(node);

            _objectIndex[id] = node;
            buffer.Add(new AutomationItem
            {
                Id = id,
                Name = name,
                Type = "TechnologicalObject",
                Path = path,
                Settings = settings
            });
        }

        foreach (var child in EnumerateChildren(node))
        {
            Traverse(child, buffer);
        }
    }

    private void CreateTechnologicalObject(string payload)
    {
        var map = ParsePayload(payload);
        var name = map.TryGetValue("Name", out var n) ? n : "TO_New";
        var type = map.TryGetValue("Type", out var t) ? t : "GenericTO";
        var path = map.TryGetValue("Path", out var p) ? p : "TechnologicalObjects";
        var settings = map.TryGetValue("Settings", out var serialized)
            ? ParseSettings(serialized)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var container = ReflectionGet(_project, "TechnologicalObjects") ?? _project;
        var methods = container.GetType().GetMethods();
        var createMethod = methods.FirstOrDefault(m => string.Equals(m.Name, "Create", StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length >= 2)
            ?? methods.FirstOrDefault(m => string.Equals(m.Name, "Add", StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length >= 1);

        if (createMethod is null)
        {
            throw new InvalidOperationException("Unable to locate Create/Add method for technological objects in current project API.");
        }

        var parameters = createMethod.GetParameters();
        object? created = parameters.Length switch
        {
            1 => createMethod.Invoke(container, new object?[] { name }),
            2 => createMethod.Invoke(container, new object?[] { type, name }),
            _ => createMethod.Invoke(container, new object?[] { type, name, path })
        };

        if (created is not null)
        {
            foreach (var kvp in settings)
            {
                SetSetting(created, kvp.Key, kvp.Value);
            }
        }
    }

    private static Dictionary<string, string> ParsePayload(string payload)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var segment in payload.Split(';'))
        {
            var idx = segment.IndexOf('=');
            if (idx <= 0 || idx == segment.Length - 1)
            {
                continue;
            }

            var key = segment.Substring(0, idx).Trim();
            var val = segment[(idx + 1)..].Trim();
            map[key] = val;
        }

        return map;
    }

    private static Dictionary<string, string> ParseSettings(string serialized)
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in serialized.Split('|'))
        {
            var idx = pair.IndexOf('=');
            if (idx <= 0)
            {
                continue;
            }

            settings[pair.Substring(0, idx)] = pair[(idx + 1)..];
        }

        return settings;
    }

    private static IDictionary<string, string> ReadSettings(object node)
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var settingsObject = ReflectionGet(node, "Settings") ?? ReflectionGet(node, "Parameters") ?? ReflectionGet(node, "ParameterValues");
        if (settingsObject is null)
        {
            return settings;
        }

        if (settingsObject is IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                if (entry.Key is not null)
                {
                    settings[entry.Key.ToString() ?? string.Empty] = entry.Value?.ToString() ?? string.Empty;
                }
            }

            return settings;
        }

        foreach (var item in EnumerateChildren(settingsObject))
        {
            var key = ReflectionGet(item, "Name")?.ToString() ?? ReflectionGet(item, "Key")?.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var value = ReflectionGet(item, "Value")?.ToString() ?? string.Empty;
            settings[key] = value;
        }

        return settings;
    }

    private static void SetSetting(object target, string key, string value)
    {
        var settingsObject = ReflectionGet(target, "Settings") ?? ReflectionGet(target, "Parameters") ?? ReflectionGet(target, "ParameterValues");
        if (settingsObject is null)
        {
            return;
        }

        if (settingsObject is IDictionary dict)
        {
            dict[key] = value;
            return;
        }

        var indexer = settingsObject.GetType().GetProperty("Item", new[] { typeof(string) });
        if (indexer?.CanWrite == true)
        {
            indexer.SetValue(settingsObject, value, new object[] { key });
        }
    }

    private static bool IsTechnologicalObject(object node)
    {
        var name = node.GetType().Name;
        return name.Contains("TechnologicalObject", StringComparison.OrdinalIgnoreCase)
               || name.Equals("TO", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<object> EnumerateChildren(object node)
    {
        var props = node.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? value;
            try
            {
                value = prop.GetValue(node);
            }
            catch
            {
                continue;
            }

            if (value is null || value is string)
            {
                continue;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is not null)
                    {
                        yield return item;
                    }
                }

                continue;
            }

            yield return value;
        }
    }

    private static object? ReflectionGet(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName);
        return prop?.GetValue(target);
    }

    private static void ReflectionSet(object target, string propertyName, object? value)
    {
        var prop = target.GetType().GetProperty(propertyName);
        if (prop?.CanWrite == true)
        {
            prop.SetValue(target, value);
        }
    }
}
