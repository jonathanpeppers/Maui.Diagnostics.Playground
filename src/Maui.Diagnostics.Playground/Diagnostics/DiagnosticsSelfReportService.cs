using System.Runtime.InteropServices;

namespace Maui.Diagnostics.Playground.Diagnostics;

public sealed class DiagnosticsSelfReportService : IDiagnosticsSelfReportService
{
    public DiagnosticsSummary GetSummary()
    {
        var runtimeFamily = Type.GetType("Mono.Runtime") is null ? "CoreCLR" : "Mono";
        var targetRuntime = IsEnabled(GeneratedBuildInfo.MauiDiagnosticsUseCoreClr) ? "CoreCLR requested" : "Mono requested";
        var platform = DeviceInfo.Current.Platform.ToString();
        var chips = new[]
        {
            runtimeFamily,
            targetRuntime,
            platform,
            GeneratedBuildInfo.Configuration,
            $"Vendor: {GeneratedBuildInfo.CrashVendor}"
        };

        var facts = new[]
        {
            new DiagnosticFact("Runtime family", runtimeFamily),
            new DiagnosticFact("Framework", RuntimeInformation.FrameworkDescription),
            new DiagnosticFact("OS", RuntimeInformation.OSDescription),
            new DiagnosticFact("Architecture", RuntimeInformation.ProcessArchitecture.ToString()),
            new DiagnosticFact("Device platform", platform),
            new DiagnosticFact("Target framework", EmptyFallback(GeneratedBuildInfo.TargetFramework, "Unknown")),
            new DiagnosticFact("Runtime identifier", EmptyFallback(GeneratedBuildInfo.RuntimeIdentifier, "Default")),
            new DiagnosticFact("Build configuration", GeneratedBuildInfo.Configuration),
            new DiagnosticFact("Crash vendor", GeneratedBuildInfo.CrashVendor),
            new DiagnosticFact("CoreCLR requested", GeneratedBuildInfo.MauiDiagnosticsUseCoreClr),
            new DiagnosticFact("UseMonoRuntime", GeneratedBuildInfo.UseMonoRuntime),
            new DiagnosticFact("Compact frame cap", GeneratedBuildInfo.CrashReportFrameLimitPerThread)
        };

        return new DiagnosticsSummary(
            runtimeFamily,
            RuntimeInformation.FrameworkDescription,
            GeneratedBuildInfo.CrashVendor,
            GeneratedBuildInfo.Configuration,
            chips,
            facts);
    }

    private static bool IsEnabled(string value)
        => bool.TryParse(value, out var result) && result;

    private static string EmptyFallback(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
