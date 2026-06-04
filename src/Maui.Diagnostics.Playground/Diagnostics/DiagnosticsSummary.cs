namespace Maui.Diagnostics.Playground.Diagnostics;

public sealed record DiagnosticsSummary(
    string RuntimeFamily,
    string RuntimeDescription,
    string ActiveVendor,
    string BuildConfiguration,
    IReadOnlyList<string> Chips,
    IReadOnlyList<DiagnosticFact> Facts);
