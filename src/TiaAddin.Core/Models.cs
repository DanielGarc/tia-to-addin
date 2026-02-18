using System;
using System.Collections.Generic;

namespace TiaAddin.Core;

public sealed class AddinConfig
{
    public bool EnableDryRunByDefault { get; set; } = true;
    public bool BlockOnValidationErrors { get; set; } = true;
    public string ReportOutputDirectory { get; set; } = "reports";
    public int MaxItemsPerBatch { get; set; } = 10000;
}

public sealed class AutomationItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public override string ToString() => $"{Type}:{Path}/{Name} ({Id})";
}

public sealed class ValidationIssue
{
    public string RuleId { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}

public sealed class OperationRequest
{
    public string OperationName { get; set; } = string.Empty;
    public bool? DryRun { get; set; }
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class ChangeSet
{
    public List<ChangeAction> Actions { get; } = new();
    public bool IsEmpty => Actions.Count == 0;
}

public sealed class ChangeAction
{
    public string ItemId { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
}

public sealed class OperationResult
{
    public bool Success { get; set; }
    public bool DryRun { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string ReportPath { get; set; } = string.Empty;
    public List<ValidationIssue> ValidationIssues { get; } = new();
    public ChangeSet Changes { get; } = new();
}
