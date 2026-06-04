namespace Maui.Diagnostics.Playground.Features.Scenarios;

public sealed class CrashScenarioCatalog : ICrashScenarioCatalog
{
    private readonly IReadOnlyList<CrashScenarioDescriptor> scenarios =
    [
        new(
            "managed-ui-unhandled",
            "Unhandled managed exception",
            "Throws from the UI thread to validate managed exception capture, runtime compact reporting, and vendor unhandled-exception handlers.",
            CrashScenarioCategory.Managed,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonManagedArtifacts,
            ["managed", "ui-thread", "unhandled"]),
        new(
            "managed-background-unhandled",
            "Background thread exception",
            "Throws from a background task to compare UI-thread and worker-thread crash reporting.",
            CrashScenarioCategory.Managed,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonManagedArtifacts,
            ["managed", "background", "task"]),
        new(
            "runtime-failfast",
            "Environment.FailFast",
            "Terminates through the runtime fatal path and should produce a compact report with managed frames on CoreCLR.",
            CrashScenarioCategory.Runtime,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonRuntimeArtifacts,
            ["runtime", "failfast", "fatal"]),
        new(
            "runtime-stack-overflow",
            "Stack overflow",
            "Recurses until stack exhaustion to validate runtime stack overflow reporting and frame limits.",
            CrashScenarioCategory.Runtime,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonRuntimeArtifacts,
            ["runtime", "stackoverflow"]),
        new(
            "native-apple-abort",
            "Apple native abort",
            "Calls into the linked Xcode framework and aborts from native code on iOS or Mac Catalyst.",
            CrashScenarioCategory.Native,
            CrashScenarioPlatform.Apple,
            typeof(ScenarioDetailPage),
            AppleNativeArtifacts,
            ["native", "apple", "abort"]),
        new(
            "native-android-sigsegv",
            "Android native SIGSEGV",
            "Calls into the linked Android native library and triggers a segmentation fault.",
            CrashScenarioCategory.Native,
            CrashScenarioPlatform.Android,
            typeof(ScenarioDetailPage),
            AndroidNativeArtifacts,
            ["native", "android", "sigsegv"]),
        new(
            "mixed-managed-native",
            "Mixed managed/native stack",
            "Bounces through managed and native frames before crashing so symbolication can validate interleaved stacks.",
            CrashScenarioCategory.Native,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonRuntimeArtifacts,
            ["native", "managed", "interleaved"]),
        new(
            "vendor-handled-exception",
            "Vendor handled exception",
            "Captures a handled exception through the active vendor abstraction without terminating the process.",
            CrashScenarioCategory.Vendor,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            ["Vendor event", "App remains running", "Scenario metadata attached"],
            ["vendor", "handled"]),
        new(
            "lifecycle-startup-crash",
            "Startup lifecycle crash",
            "Crashes early in app startup to verify what handlers are installed before the first page renders.",
            CrashScenarioCategory.Lifecycle,
            CrashScenarioPlatform.Mobile,
            typeof(ScenarioDetailPage),
            CommonManagedArtifacts,
            ["startup", "lifecycle"]),
        new(
            "edgehost-app-intent",
            "App Intent host crash",
            "Future Apple extension-host scenario for validating crash reporting outside the main MAUI app process.",
            CrashScenarioCategory.EdgeHost,
            CrashScenarioPlatform.Apple,
            typeof(ScenarioDetailPage),
            ["Separate process crash log", "Extension host runtime details", "Vendor extension support notes"],
            ["edge-host", "app-intents", "future"])
    ];

    public IReadOnlyList<CrashScenarioDescriptor> All => scenarios;

    public CrashScenarioDescriptor GetRequired(string key)
        => scenarios.FirstOrDefault(scenario => scenario.Key == key)
            ?? throw new InvalidOperationException($"Unknown crash scenario '{key}'.");

    private static readonly string[] CommonManagedArtifacts =
    [
        "Runtime compact report when CoreCLR crash reporting is configured",
        "Platform crash log or process termination record",
        "Active vendor event when vendor handlers are enabled"
    ];

    private static readonly string[] CommonRuntimeArtifacts =
    [
        "CoreCLR compact report with managed/native frames",
        "Module table with runtime module identifiers",
        "Platform crash log or tombstone"
    ];

    private static readonly string[] AppleNativeArtifacts =
    [
        "Apple .ips crash log",
        "App and native framework dSYMs",
        "CoreCLR stderr/minipal compact report when available"
    ];

    private static readonly string[] AndroidNativeArtifacts =
    [
        "Android tombstone or logcat fatal signal block",
        "DOTNET_CRASH logcat block when CoreCLR report is available",
        "ELF BuildIds for runtime and native crash kit libraries"
    ];
}
