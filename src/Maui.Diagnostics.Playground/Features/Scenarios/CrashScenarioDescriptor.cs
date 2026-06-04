namespace Maui.Diagnostics.Playground.Features.Scenarios;

public sealed record CrashScenarioDescriptor(
    string Key,
    string Title,
    string Description,
    CrashScenarioCategory Category,
    CrashScenarioPlatform Platforms,
    Type PageType,
    IReadOnlyList<string> ExpectedArtifacts,
    IReadOnlyList<string> Tags)
{
    public string CategoryLabel => Category switch
    {
        CrashScenarioCategory.EdgeHost => "Edge host",
        _ => Category.ToString()
    };

    public string PlatformLabel
    {
        get
        {
            if (Platforms == CrashScenarioPlatform.Mobile)
            {
                return "Android, iOS, Mac Catalyst";
            }

            var platforms = new List<string>();
            if (Platforms.HasFlag(CrashScenarioPlatform.Android))
            {
                platforms.Add("Android");
            }

            if (Platforms.HasFlag(CrashScenarioPlatform.iOS))
            {
                platforms.Add("iOS");
            }

            if (Platforms.HasFlag(CrashScenarioPlatform.MacCatalyst))
            {
                platforms.Add("Mac Catalyst");
            }

            return platforms.Count == 0 ? "No platforms" : string.Join(", ", platforms);
        }
    }
}
