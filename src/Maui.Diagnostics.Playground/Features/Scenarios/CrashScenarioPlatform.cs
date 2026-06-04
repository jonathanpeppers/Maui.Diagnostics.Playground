namespace Maui.Diagnostics.Playground.Features.Scenarios;

[Flags]
public enum CrashScenarioPlatform
{
    None = 0,
    Android = 1,
    iOS = 2,
    MacCatalyst = 4,
    Apple = iOS | MacCatalyst,
    Mobile = Android | iOS | MacCatalyst
}
