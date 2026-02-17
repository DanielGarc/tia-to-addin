using System;
using System.Collections.Generic;
using FluentAssertions;
using TiaAddin.Core;
using TiaAddin.Core.TechnologicalObjects;
using Xunit;

namespace TiaAddin.Core.Tests.TechnologicalObjects;

public class TechnologicalObjectFeaturesTests
{
    [Fact]
    public void TechnologicalObjectOnlyRule_ShouldFlagNonToItems()
    {
        var rule = new TechnologicalObjectOnlyRule();
        var items = new List<AutomationItem>
        {
            new() { Id = "1", Name = "Axis1", Type = "TechnologicalObject" },
            new() { Id = "2", Name = "Tag1", Type = "Tag" }
        };

        var issues = rule.Validate(items, new OperationRequest()).Should().ContainSingle().Subject;
        issues.ItemId.Should().Be("2");
        issues.RuleId.Should().Be("TO001");
    }

    [Fact]
    public void ToPrefixOperation_ShouldChangeOnlyToItems()
    {
        var operation = new PrefixTechnologicalObjectNameOperation();
        var request = new OperationRequest
        {
            Parameters = new Dictionary<string, string> { ["prefix"] = "TO_" }
        };
        var items = new List<AutomationItem>
        {
            new() { Id = "1", Name = "Axis1", Type = "TechnologicalObject" },
            new() { Id = "2", Name = "Tag1", Type = "Tag" }
        };

        var changes = operation.Plan(items, request);

        changes.Actions.Should().ContainSingle();
        changes.Actions[0].ItemId.Should().Be("1");
        changes.Actions[0].NewValue.Should().Be("TO_Axis1");
    }

    [Fact]
    public void CreateToOperation_ShouldPlanRequestedCount()
    {
        var operation = new CreateTechnologicalObjectsOperation();
        var request = new OperationRequest
        {
            Parameters = new Dictionary<string, string>
            {
                ["baseName"] = "Axis",
                ["count"] = "2",
                ["toType"] = "AxisTO"
            }
        };

        var changes = operation.Plan(new List<AutomationItem>(), request);
        changes.Actions.Should().HaveCount(2);
        changes.Actions[0].Field.Should().Be("CreateTechnologicalObject");
    }

    [Fact]
    public void SyncSettingsOperation_ShouldCopySourceSettingsToTargets()
    {
        var operation = new SyncTechnologicalObjectSettingsOperation();
        var items = new List<AutomationItem>
        {
            new() { Id = "S", Name = "Source", Type = "TechnologicalObject", Settings = new Dictionary<string,string>{{"Speed","1500"}} },
            new() { Id = "T1", Name = "Target", Type = "TechnologicalObject", Settings = new Dictionary<string,string>{{"Speed","1000"}} }
        };

        var request = new OperationRequest
        {
            Parameters = new Dictionary<string, string>
            {
                ["sourceId"] = "S",
                ["targetIds"] = "T1"
            }
        };

        var changes = operation.Plan(items, request);
        changes.Actions.Should().ContainSingle();
        changes.Actions[0].Field.Should().Be("Setting:Speed");
        changes.Actions[0].NewValue.Should().Be("1500");
    }

    [Fact]
    public void BulkConfigureOperation_ShouldGenerateSettingUpdates()
    {
        var operation = new BulkConfigureTechnologicalObjectsOperation();
        var items = new List<AutomationItem>
        {
            new() { Id = "T1", Name = "Target", Type = "TechnologicalObject", Settings = new Dictionary<string,string>{{"Jerk","10"}} }
        };

        var request = new OperationRequest
        {
            Parameters = new Dictionary<string, string>
            {
                ["targetIds"] = "T1",
                ["setting.Jerk"] = "25"
            }
        };

        var changes = operation.Plan(items, request);
        changes.Actions.Should().ContainSingle();
        changes.Actions[0].Field.Should().Be("Setting:Jerk");
        changes.Actions[0].OldValue.Should().Be("10");
        changes.Actions[0].NewValue.Should().Be("25");
    }
}
