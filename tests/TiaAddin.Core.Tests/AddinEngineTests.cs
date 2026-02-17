using System;
using System.Collections.Generic;
using FluentAssertions;
using TiaAddin.Core;
using Xunit;

namespace TiaAddin.Core.Tests;

public class AddinEngineTests
{
    [Fact]
    public void Execute_ShouldPlanChangesInDryRun()
    {
        var gateway = new FakeGateway(new List<AutomationItem>
        {
            new() { Id = "1", Name = "Motor01", Type = "Tag", Path = "Tags/Main" }
        });

        var config = new AddinConfig { EnableDryRunByDefault = true, BlockOnValidationErrors = true };
        var engine = CreateEngine(config, gateway);

        var result = engine.Execute(new OperationRequest
        {
            OperationName = "prefix-name",
            Parameters = new Dictionary<string, string> { ["prefix"] = "P_" }
        });

        result.Success.Should().BeTrue();
        result.DryRun.Should().BeTrue();
        result.Changes.Actions.Should().ContainSingle();
        gateway.ApplyCallCount.Should().Be(0);
    }

    [Fact]
    public void Execute_ShouldApplyChangesWhenNotDryRun()
    {
        var gateway = new FakeGateway(new List<AutomationItem>
        {
            new() { Id = "1", Name = "line_a", Type = "Tag", Path = "Tags/Main" }
        });

        var config = new AddinConfig { EnableDryRunByDefault = false, BlockOnValidationErrors = true };
        var engine = CreateEngine(config, gateway);

        var result = engine.Execute(new OperationRequest
        {
            OperationName = "uppercase-name"
        });

        result.Success.Should().BeTrue();
        gateway.ApplyCallCount.Should().Be(1);
        gateway.Items[0].Name.Should().Be("LINE_A");
    }

    [Fact]
    public void Execute_ShouldBlockOnValidationErrors()
    {
        var gateway = new FakeGateway(new List<AutomationItem>
        {
            new() { Id = "1", Name = "", Type = "Tag", Path = "Tags/Main" }
        });

        var config = new AddinConfig { EnableDryRunByDefault = false, BlockOnValidationErrors = true };
        var engine = CreateEngine(config, gateway);

        var result = engine.Execute(new OperationRequest
        {
            OperationName = "uppercase-name"
        });

        result.Success.Should().BeFalse();
        result.ValidationIssues.Should().NotBeEmpty();
        gateway.ApplyCallCount.Should().Be(0);
    }

    private static AddinEngine CreateEngine(AddinConfig config, FakeGateway gateway)
    {
        var reportWriter = new InMemoryReportWriter();
        var logger = new InMemoryLogger();

        return new AddinEngine(
            config,
            gateway,
            OperationCatalog.Default,
            new IValidationRule[] { new RequiredNameRule(), new DuplicateNameRule(), new NameLengthRule() },
            reportWriter,
            logger);
    }

    private sealed class FakeGateway : IProjectGateway
    {
        public List<AutomationItem> Items { get; }
        public int ApplyCallCount { get; private set; }

        public FakeGateway(List<AutomationItem> items)
        {
            Items = items;
        }

        public IReadOnlyList<AutomationItem> GetSelectedItems() => Items;

        public void ApplyChanges(ChangeSet changeSet)
        {
            ApplyCallCount++;
            foreach (var action in changeSet.Actions)
            {
                var item = Items.Find(i => i.Id == action.ItemId) ?? throw new InvalidOperationException("Missing item");
                if (action.Field == "Name")
                {
                    item.Name = action.NewValue;
                }
            }
        }
    }

    private sealed class InMemoryReportWriter : IReportWriter
    {
        public string Write(OperationRequest request, OperationResult result) => "in-memory";
    }

    private sealed class InMemoryLogger : ILogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }
}
