using System.Windows.Input;

namespace Maui.Diagnostics.Playground.Features.Scenarios;

public partial class ScenarioDetailPage : ContentPage, IQueryAttributable
{
    public const string Route = "scenario";

    private readonly ICrashScenarioCatalog scenarioCatalog;
    private readonly ICrashScenarioRunner scenarioRunner;
    private CrashScenarioDescriptor? scenario;
    private bool canRunScenario;

    public ScenarioDetailPage(ICrashScenarioCatalog scenarioCatalog, ICrashScenarioRunner scenarioRunner)
    {
        this.scenarioCatalog = scenarioCatalog;
        this.scenarioRunner = scenarioRunner;
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        RunScenarioCommand = new Command(async () => await RunScenarioAsync(), () => CanRunScenario);
        InitializeComponent();
        BindingContext = this;
    }

    public CrashScenarioDescriptor? Scenario
    {
        get => scenario;
        private set
        {
            scenario = value;
            CanRunScenario = value is not null && scenarioRunner.CanRun(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActionTitle));
            OnPropertyChanged(nameof(ActionDescription));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(ActionBackgroundColor));
            OnPropertyChanged(nameof(ActionStrokeColor));
            OnPropertyChanged(nameof(ActionTextColor));
        }
    }

    public ICommand BackCommand { get; }

    public ICommand RunScenarioCommand { get; }

    public bool CanRunScenario
    {
        get => canRunScenario;
        private set
        {
            canRunScenario = value;
            OnPropertyChanged();
            ((Command)RunScenarioCommand).ChangeCanExecute();
        }
    }

    public string ActionTitle => CanRunScenario ? "Danger zone" : "Crash action not wired yet";

    public string ActionDescription => CanRunScenario
        ? "This scenario intentionally terminates or destabilizes the app. Confirm only after attaching the diagnostics collection tools you want to validate."
        : "The descriptor is registered in the gallery. A later implementation phase will connect this scenario to a managed, native, or vendor-specific runner.";

    public string ActionButtonText => CanRunScenario ? "Trigger scenario" : "Planned";

    public Color ActionBackgroundColor => CanRunScenario ? Color.FromArgb("#FEF2F2") : Color.FromArgb("#FFF7ED");

    public Color ActionStrokeColor => CanRunScenario ? Color.FromArgb("#FCA5A5") : Color.FromArgb("#FDBA74");

    public Color ActionTextColor => CanRunScenario ? Color.FromArgb("#991B1B") : Color.FromArgb("#9A3412");

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("scenarioKey", out var value) || value is not string key)
        {
            throw new InvalidOperationException("Scenario navigation requires a scenarioKey query parameter.");
        }

        Scenario = scenarioCatalog.GetRequired(key);
        Title = Scenario.Title;
    }

    private async Task RunScenarioAsync()
    {
        if (Scenario is null)
        {
            throw new InvalidOperationException("Cannot run a crash scenario before navigation supplies a descriptor.");
        }

        var confirmed = await DisplayAlertAsync(
            "Trigger crash scenario?",
            $"This will run '{Scenario.Title}' and may immediately terminate the app.",
            "Trigger",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        await scenarioRunner.RunAsync(Scenario);
    }
}
