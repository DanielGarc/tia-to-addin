using System;
using System.Collections.Generic;
using System.Linq;

namespace TiaAddin.Core.TechnologicalObjects;

public sealed class PrefixTechnologicalObjectNameOperation : IOperation
{
    public string Name => "to-prefix-name";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        if (!request.Parameters.TryGetValue("prefix", out var prefix) || string.IsNullOrWhiteSpace(prefix))
        {
            throw new InvalidOperationException("Parameter 'prefix' is required for to-prefix-name operation.");
        }

        var changeSet = new ChangeSet();
        foreach (var item in items.Where(i => IsTo(i)))
        {
            changeSet.Actions.Add(new ChangeAction
            {
                ItemId = item.Id,
                Field = "Name",
                OldValue = item.Name,
                NewValue = $"{prefix}{item.Name}"
            });
        }

        return changeSet;
    }

    private static bool IsTo(AutomationItem item) => string.Equals(item.Type, "TechnologicalObject", StringComparison.OrdinalIgnoreCase);
}

public sealed class CreateTechnologicalObjectsOperation : IOperation
{
    public string Name => "to-create";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var baseName = request.Parameters.TryGetValue("baseName", out var bn) ? bn : "TO";
        var toType = request.Parameters.TryGetValue("toType", out var tt) ? tt : "GenericTO";
        var path = request.Parameters.TryGetValue("path", out var p) ? p : "TechnologicalObjects";
        var count = request.Parameters.TryGetValue("count", out var raw) && int.TryParse(raw, out var c) ? c : 1;
        if (count < 1)
        {
            throw new InvalidOperationException("Parameter 'count' must be >= 1 for to-create operation.");
        }

        var settings = request.Parameters
            .Where(kvp => kvp.Key.StartsWith("setting.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key.Substring("setting.".Length), kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        var changeSet = new ChangeSet();
        for (var i = 1; i <= count; i++)
        {
            var name = $"{baseName}{i:00}";
            var serializedSettings = string.Join("|", settings.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            changeSet.Actions.Add(new ChangeAction
            {
                ItemId = $"new:{name}",
                Field = "CreateTechnologicalObject",
                OldValue = string.Empty,
                NewValue = $"Name={name};Type={toType};Path={path};Settings={serializedSettings}"
            });
        }

        return changeSet;
    }
}

public sealed class BulkConfigureTechnologicalObjectsOperation : IOperation
{
    private static bool IsTo(AutomationItem item) => string.Equals(item.Type, "TechnologicalObject", StringComparison.OrdinalIgnoreCase);

    public string Name => "to-bulk-configure";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var settings = request.Parameters
            .Where(kvp => kvp.Key.StartsWith("setting.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key.Substring("setting.".Length), kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        if (settings.Count == 0)
        {
            throw new InvalidOperationException("At least one 'setting.<key>' parameter is required for to-bulk-configure operation.");
        }

        var targetIds = request.Parameters.TryGetValue("targetIds", out var rawIds)
            ? new HashSet<string>(rawIds.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var targets = items.Where(IsTo).Where(i => targetIds.Count == 0 || targetIds.Contains(i.Id));
        var changeSet = new ChangeSet();

        foreach (var target in targets)
        {
            foreach (var kvp in settings)
            {
                target.Settings.TryGetValue(kvp.Key, out var oldValue);
                if (string.Equals(oldValue, kvp.Value, StringComparison.Ordinal))
                {
                    continue;
                }

                changeSet.Actions.Add(new ChangeAction
                {
                    ItemId = target.Id,
                    Field = $"Setting:{kvp.Key}",
                    OldValue = oldValue ?? string.Empty,
                    NewValue = kvp.Value
                });
            }
        }

        return changeSet;
    }
}

public sealed class SyncTechnologicalObjectSettingsOperation : IOperation
{
    public string Name => "to-sync-settings";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var sourceId = request.Parameters.TryGetValue("sourceId", out var src) ? src : string.Empty;
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new InvalidOperationException("Parameter 'sourceId' is required for to-sync-settings operation.");
        }

        var source = items.FirstOrDefault(i => IsTo(i) && string.Equals(i.Id, sourceId, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            throw new InvalidOperationException($"Source TO '{sourceId}' not found in selection.");
        }

        var includeKeys = request.Parameters.TryGetValue("keys", out var rawKeys)
            ? new HashSet<string>(rawKeys.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(source.Settings.Keys, StringComparer.OrdinalIgnoreCase);

        var targetIds = request.Parameters.TryGetValue("targetIds", out var rawIds)
            ? new HashSet<string>(rawIds.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var targets = items
            .Where(IsTo)
            .Where(i => !string.Equals(i.Id, source.Id, StringComparison.OrdinalIgnoreCase))
            .Where(i => targetIds.Count == 0 || targetIds.Contains(i.Id));

        var changeSet = new ChangeSet();
        foreach (var target in targets)
        {
            foreach (var key in includeKeys)
            {
                if (!source.Settings.TryGetValue(key, out var sourceValue))
                {
                    continue;
                }

                target.Settings.TryGetValue(key, out var oldValue);
                if (string.Equals(sourceValue, oldValue, StringComparison.Ordinal))
                {
                    continue;
                }

                changeSet.Actions.Add(new ChangeAction
                {
                    ItemId = target.Id,
                    Field = $"Setting:{key}",
                    OldValue = oldValue ?? string.Empty,
                    NewValue = sourceValue
                });
            }
        }

        return changeSet;
    }

    private static bool IsTo(AutomationItem item) => string.Equals(item.Type, "TechnologicalObject", StringComparison.OrdinalIgnoreCase);
}
