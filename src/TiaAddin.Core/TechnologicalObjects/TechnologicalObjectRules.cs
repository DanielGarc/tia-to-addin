using System;
using System.Collections.Generic;
using System.Linq;

namespace TiaAddin.Core.TechnologicalObjects;

public sealed class TechnologicalObjectOnlyRule : IValidationRule
{
    public string RuleId => "TO001";

    public IEnumerable<ValidationIssue> Validate(IReadOnlyList<AutomationItem> items, OperationRequest request)
    {
        return items
            .Where(i => !string.Equals(i.Type, "TechnologicalObject", StringComparison.OrdinalIgnoreCase))
            .Select(i => new ValidationIssue
            {
                RuleId = RuleId,
                Severity = "Error",
                ItemId = i.Id,
                Message = "Selected item is not a Technological Object."
            });
    }
}
