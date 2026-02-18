using System.Collections.Generic;
using System.Linq;
using TiaAddin.Core;

namespace TiaAddin.TiaHost;

public sealed class TechnologicalObjectAddinService
{
    private readonly AddinEngine _engine;

    public TechnologicalObjectAddinService(AddinEngine engine)
    {
        _engine = engine;
    }

    public OperationResult PrefixTechnologicalObjectNames(string prefix, bool dryRun)
    {
        return _engine.Execute(new OperationRequest
        {
            OperationName = "to-prefix-name",
            DryRun = dryRun,
            Parameters = new Dictionary<string, string>
            {
                ["prefix"] = prefix
            }
        });
    }

    public OperationResult CreateTechnologicalObjects(string baseName, string toType, string path, int count, IDictionary<string, string>? settings, bool dryRun)
    {
        var parameters = new Dictionary<string, string>
        {
            ["baseName"] = baseName,
            ["toType"] = toType,
            ["path"] = path,
            ["count"] = count.ToString()
        };

        foreach (var kvp in settings ?? new Dictionary<string, string>())
        {
            parameters[$"setting.{kvp.Key}"] = kvp.Value;
        }

        return _engine.Execute(new OperationRequest
        {
            OperationName = "to-create",
            DryRun = dryRun,
            Parameters = parameters
        });
    }

    public OperationResult SyncSettingsFromSourceToTargets(string sourceId, IEnumerable<string> targetIds, IEnumerable<string>? keys, bool dryRun)
    {
        var parameters = new Dictionary<string, string>
        {
            ["sourceId"] = sourceId,
            ["targetIds"] = string.Join(",", targetIds)
        };

        var keyList = keys?.ToArray() ?? new string[0];
        if (keyList.Length > 0)
        {
            parameters["keys"] = string.Join(",", keyList);
        }

        return _engine.Execute(new OperationRequest
        {
            OperationName = "to-sync-settings",
            DryRun = dryRun,
            Parameters = parameters
        });
    }

    public OperationResult BulkConfigureTechnologicalObjects(IEnumerable<string> targetIds, IDictionary<string, string> settings, bool dryRun)
    {
        var parameters = new Dictionary<string, string>
        {
            ["targetIds"] = string.Join(",", targetIds)
        };

        foreach (var kvp in settings)
        {
            parameters[$"setting.{kvp.Key}"] = kvp.Value;
        }

        return _engine.Execute(new OperationRequest
        {
            OperationName = "to-bulk-configure",
            DryRun = dryRun,
            Parameters = parameters
        });
    }
}
