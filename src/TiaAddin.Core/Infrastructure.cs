using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TiaAddin.Core;

public sealed class FileReportWriter : IReportWriter
{
    private readonly AddinConfig _config;

    public FileReportWriter(AddinConfig config)
    {
        _config = config;
    }

    public string Write(OperationRequest request, OperationResult result)
    {
        Directory.CreateDirectory(_config.ReportOutputDirectory);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var path = Path.Combine(_config.ReportOutputDirectory, $"{request.OperationName}-{timestamp}.md");

        var sb = new StringBuilder();
        sb.AppendLine($"# TIA Add-in Report: {request.OperationName}");
        sb.AppendLine();
        sb.AppendLine($"- Success: {result.Success}");
        sb.AppendLine($"- DryRun: {result.DryRun}");
        sb.AppendLine($"- Summary: {result.Summary}");
        sb.AppendLine();

        sb.AppendLine("## Validation Issues");
        if (result.ValidationIssues.Count == 0)
        {
            sb.AppendLine("- None");
        }
        else
        {
            foreach (var issue in result.ValidationIssues)
            {
                sb.AppendLine($"- [{issue.Severity}] {issue.RuleId} Item={issue.ItemId} :: {issue.Message}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Changes");
        if (!result.Changes.Actions.Any())
        {
            sb.AppendLine("- None");
        }
        else
        {
            foreach (var action in result.Changes.Actions)
            {
                sb.AppendLine($"- Item={action.ItemId} Field={action.Field} '{action.OldValue}' -> '{action.NewValue}'");
            }
        }

        File.WriteAllText(path, sb.ToString());
        return path;
    }
}

public sealed class TextFileLogger : ILogger
{
    private readonly string _path;

    public TextFileLogger(string path)
    {
        _path = path;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"{DateTime.UtcNow:O} [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(_path, line);
    }
}
