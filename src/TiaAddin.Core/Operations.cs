using System;
using System.Collections.Generic;
using System.Linq;
using TiaAddin.Core.TechnologicalObjects;

namespace TiaAddin.Core;

public sealed class PrefixNameOperation : IOperation
{
    public string Name => "prefix-name";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        if (!request.Parameters.TryGetValue("prefix", out var prefix) || string.IsNullOrWhiteSpace(prefix))
        {
            throw new InvalidOperationException("Parameter 'prefix' is required for prefix-name operation.");
        }

        var changeSet = new ChangeSet();
        foreach (var item in items)
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
}

public sealed class ReplaceTextInNameOperation : IOperation
{
    public string Name => "replace-name";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        request.Parameters.TryGetValue("from", out var from);
        request.Parameters.TryGetValue("to", out var to);

        if (string.IsNullOrEmpty(from))
        {
            throw new InvalidOperationException("Parameter 'from' is required for replace-name operation.");
        }

        to ??= string.Empty;

        var changeSet = new ChangeSet();
        foreach (var item in items)
        {
            var newValue = item.Name.Replace(from, to, StringComparison.Ordinal);
            if (newValue == item.Name)
            {
                continue;
            }

            changeSet.Actions.Add(new ChangeAction
            {
                ItemId = item.Id,
                Field = "Name",
                OldValue = item.Name,
                NewValue = newValue
            });
        }

        return changeSet;
    }
}

public sealed class EnforceUppercaseOperation : IOperation
{
    public string Name => "uppercase-name";

    public ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var changeSet = new ChangeSet();
        foreach (var item in items)
        {
            var upper = item.Name.ToUpperInvariant();
            if (upper == item.Name)
            {
                continue;
            }

            changeSet.Actions.Add(new ChangeAction
            {
                ItemId = item.Id,
                Field = "Name",
                OldValue = item.Name,
                NewValue = upper
            });
        }

        return changeSet;
    }
}

public static class OperationCatalog
{
    public static IReadOnlyList<IOperation> Default { get; } = new IOperation[]
    {
        new PrefixNameOperation(),
        new ReplaceTextInNameOperation(),
        new EnforceUppercaseOperation(),
        new PrefixTechnologicalObjectNameOperation(),
        new CreateTechnologicalObjectsOperation(),
        new SyncTechnologicalObjectSettingsOperation(),
        new BulkConfigureTechnologicalObjectsOperation()
    };
}
