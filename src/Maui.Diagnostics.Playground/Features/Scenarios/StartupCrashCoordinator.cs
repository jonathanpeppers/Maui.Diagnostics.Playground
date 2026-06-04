namespace Maui.Diagnostics.Playground.Features.Scenarios;

public static class StartupCrashCoordinator
{
    private const string StartupCrashArmedKey = "MauiDiagnostics.Playground.StartupCrashArmed";

    public static void Arm()
    {
        Preferences.Default.Set(StartupCrashArmedKey, true);
    }

    public static void CrashIfArmed()
    {
        if (!Preferences.Default.Get(StartupCrashArmedKey, false))
        {
            return;
        }

        Preferences.Default.Remove(StartupCrashArmedKey);
        throw new InvalidOperationException("Simulated startup lifecycle crash before the first Shell page is created.");
    }
}
