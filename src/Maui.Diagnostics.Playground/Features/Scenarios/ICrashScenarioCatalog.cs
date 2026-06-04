namespace Maui.Diagnostics.Playground.Features.Scenarios;

public interface ICrashScenarioCatalog
{
    IReadOnlyList<CrashScenarioDescriptor> All { get; }

    CrashScenarioDescriptor GetRequired(string key);
}
