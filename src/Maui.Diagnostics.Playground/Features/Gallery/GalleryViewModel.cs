using System.Windows.Input;
using Maui.Diagnostics.Playground.Diagnostics;
using Maui.Diagnostics.Playground.Features.Scenarios;

namespace Maui.Diagnostics.Playground.Features.Gallery;

public sealed class GalleryViewModel
{
    public GalleryViewModel(
        ICrashScenarioCatalog scenarioCatalog,
        IDiagnosticsSelfReportService diagnosticsSelfReportService)
    {
        Scenarios = scenarioCatalog.All
            .OrderBy(scenario => scenario.Category)
            .ThenBy(scenario => scenario.Title)
            .ToArray();
        Summary = diagnosticsSelfReportService.GetSummary();
        OpenScenarioCommand = new Command<CrashScenarioDescriptor>(OpenScenario);
    }

    public IReadOnlyList<CrashScenarioDescriptor> Scenarios { get; }

    public DiagnosticsSummary Summary { get; }

    public ICommand OpenScenarioCommand { get; }

    public string ScenarioCountText => $"{Scenarios.Count} scenarios registered from the central catalog";

    private static async void OpenScenario(CrashScenarioDescriptor? scenario)
    {
        if (scenario is null)
        {
            return;
        }

        await Shell.Current.GoToAsync(ScenarioDetailPage.Route, new ShellNavigationQueryParameters
        {
            { "scenarioKey", scenario.Key }
        });
    }
}
