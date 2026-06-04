namespace Maui.Diagnostics.Playground.Features.Scenarios;

public static class StartupCrashCoordinator
{
    private const string StartupCrashArmedKey = "MauiDiagnostics.Playground.StartupCrashArmed";
    private const string ResumeCrashArmedKey = "MauiDiagnostics.Playground.ResumeCrashArmed";

    public static void Arm()
    {
        Preferences.Default.Set(StartupCrashArmedKey, true);
    }

    public static void ArmResumeCrash()
    {
        Preferences.Default.Set(ResumeCrashArmedKey, true);
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

    public static void CrashOnResumeIfArmed()
    {
        if (!Preferences.Default.Get(ResumeCrashArmedKey, false))
        {
            return;
        }

        Preferences.Default.Remove(ResumeCrashArmedKey);
        throw new InvalidOperationException("Simulated resume lifecycle crash after returning from the background.");
    }
}
