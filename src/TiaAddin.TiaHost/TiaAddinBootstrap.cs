using System;
using System.Collections.Generic;
using TiaAddin.Core;
using TiaAddin.Core.TechnologicalObjects;

namespace TiaAddin.TiaHost;

public static class TiaAddinBootstrap
{
    public static AddinEngine CreateForTechnologicalObjects(
        object engineeringProject,
        string addInDllPath,
        string engineeringDllPath,
        string reportDirectory,
        string logFile)
    {
        var guard = new TiaDependencyGuard();
        guard.EnsureAssemblyLoaded(addInDllPath, "AddIn");
        guard.EnsureAssemblyLoaded(engineeringDllPath, "Siemens.Engineering");

        var config = new AddinConfig
        {
            EnableDryRunByDefault = true,
            BlockOnValidationErrors = true,
            ReportOutputDirectory = reportDirectory
        };

        var gateway = new TiaReflectionGateway(engineeringProject);
        var reportWriter = new FileReportWriter(config);
        var logger = new TextFileLogger(logFile);

        var rules = new List<IValidationRule>
        {
            new TechnologicalObjectOnlyRule(),
            new RequiredNameRule(),
            new DuplicateNameRule(),
            new NameLengthRule()
        };

        return new AddinEngine(config, gateway, OperationCatalog.Default, rules, reportWriter, logger);
    }
}
