using System;
using System.Collections.Generic;
using System.Linq;

namespace TiaAddin.Core;

public sealed class AddinEngine
{
    private readonly AddinConfig _config;
    private readonly IProjectGateway _gateway;
    private readonly IReadOnlyDictionary<string, IOperation> _operations;
    private readonly IReadOnlyList<IValidationRule> _rules;
    private readonly IReportWriter _reportWriter;
    private readonly ILogger _logger;

    public AddinEngine(
        AddinConfig config,
        IProjectGateway gateway,
        IEnumerable<IOperation> operations,
        IEnumerable<IValidationRule> rules,
        IReportWriter reportWriter,
        ILogger logger)
    {
        _config = config;
        _gateway = gateway;
        _operations = operations.ToDictionary(op => op.Name, StringComparer.OrdinalIgnoreCase);
        _rules = rules.ToList();
        _reportWriter = reportWriter;
        _logger = logger;
    }

    public OperationResult Execute(OperationRequest request)
    {
        if (!_operations.TryGetValue(request.OperationName, out var operation))
        {
            throw new InvalidOperationException($"Unknown operation: {request.OperationName}");
        }

        var result = new OperationResult();
        var dryRun = request.DryRun ?? _config.EnableDryRunByDefault;
        result.DryRun = dryRun;

        var items = _gateway.GetSelectedItems();
        if (items.Count > _config.MaxItemsPerBatch)
        {
            throw new InvalidOperationException($"Selected items ({items.Count}) exceed max batch limit ({_config.MaxItemsPerBatch}).");
        }

        _logger.Info($"Executing operation '{request.OperationName}' for {items.Count} item(s). DryRun={dryRun}.");

        foreach (var rule in _rules)
        {
            result.ValidationIssues.AddRange(rule.Validate(items, request));
        }

        var hasValidationErrors = result.ValidationIssues.Any(i => string.Equals(i.Severity, "Error", StringComparison.OrdinalIgnoreCase));
        if (hasValidationErrors && _config.BlockOnValidationErrors)
        {
            result.Success = false;
            result.Summary = "Validation blocked operation execution.";
            result.ReportPath = _reportWriter.Write(request, result);
            _logger.Warn(result.Summary);
            return result;
        }

        var changeSet = operation.Plan(items, request);
        result.Changes.Actions.AddRange(changeSet.Actions);

        if (!dryRun && !changeSet.IsEmpty)
        {
            _gateway.ApplyChanges(changeSet);
            _logger.Info($"Applied {changeSet.Actions.Count} change(s).");
        }

        result.Success = true;
        result.Summary = changeSet.IsEmpty ? "No changes required." : $"Planned {changeSet.Actions.Count} change(s).";
        result.ReportPath = _reportWriter.Write(request, result);

        return result;
    }
}
