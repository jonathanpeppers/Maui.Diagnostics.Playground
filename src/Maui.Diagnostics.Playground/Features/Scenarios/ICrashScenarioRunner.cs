namespace Maui.Diagnostics.Playground.Features.Scenarios;

public interface ICrashScenarioRunner
{
    bool CanRun(CrashScenarioDescriptor scenario);

    Task RunAsync(CrashScenarioDescriptor scenario);
}
