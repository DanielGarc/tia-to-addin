using System;
using System.Collections.Generic;
using System.Linq;

namespace TiaAddin.Core;

public sealed class RequiredNameRule : IValidationRule
{
    public string RuleId => "VAL001";

    public IEnumerable<ValidationIssue> Validate(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        return items
            .Where(i => string.IsNullOrWhiteSpace(i.Name))
            .Select(i => new ValidationIssue
            {
                RuleId = RuleId,
                Severity = "Error",
                ItemId = i.Id,
                Message = "Item name is required."
            });
    }
}

public sealed class NameLengthRule : IValidationRule
{
    public string RuleId => "VAL002";

    public IEnumerable<ValidationIssue> Validate(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var maxLen = request.Parameters.TryGetValue("maxNameLength", out var raw)
            && int.TryParse(raw, out var parsed)
            ? parsed
            : 24;

        return items
            .Where(i => i.Name.Length > maxLen)
            .Select(i => new ValidationIssue
            {
                RuleId = RuleId,
                Severity = "Warning",
                ItemId = i.Id,
                Message = $"Name '{i.Name}' exceeds recommended max length of {maxLen}."
            });
    }
}

public sealed class DuplicateNameRule : IValidationRule
{
    public string RuleId => "VAL003";

    public IEnumerable<ValidationIssue> Validate(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        var duplicateGroups = items
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            foreach (var item in group)
            {
                yield return new ValidationIssue
                {
                    RuleId = RuleId,
                    Severity = "Error",
                    ItemId = item.Id,
                    Message = $"Duplicate item name detected: {group.Key}."
                };
            }
        }
    }
}
