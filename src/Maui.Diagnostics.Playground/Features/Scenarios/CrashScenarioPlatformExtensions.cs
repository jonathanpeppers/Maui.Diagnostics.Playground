namespace Maui.Diagnostics.Playground.Features.Scenarios;

public static class CrashScenarioPlatformExtensions
{
    public static bool SupportsCurrentDevice(this CrashScenarioDescriptor scenario)
        => scenario.Platforms.HasFlag(GetCurrentPlatform());

    public static CrashScenarioPlatform GetCurrentPlatform()
        => DeviceInfo.Current.Platform == DevicePlatform.Android
            ? CrashScenarioPlatform.Android
            : DeviceInfo.Current.Platform == DevicePlatform.iOS
                ? CrashScenarioPlatform.iOS
                : DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst
                    ? CrashScenarioPlatform.MacCatalyst
                    : CrashScenarioPlatform.None;
}
