using System.Collections.Generic;

namespace TiaAddin.Core;

public interface IProjectGateway
{
    IReadOnlyList<AutomationItem> GetSelectedItems();
    void ApplyChanges(ChangeSet changeSet);
}

public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<ValidationIssue> Validate(IReadOnlyList<AutomationItem> items, OperationRequest request);
}

public interface IOperation
{
    string Name { get; }
    ChangeSet Plan(IReadOnlyList<AutomationItem> items, OperationRequest request);
}

public interface IReportWriter
{
    string Write(OperationRequest request, OperationResult result);
}

public interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}
