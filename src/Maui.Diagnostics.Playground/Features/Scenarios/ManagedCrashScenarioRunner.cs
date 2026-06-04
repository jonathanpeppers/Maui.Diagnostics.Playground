using System.Runtime.CompilerServices;

namespace Maui.Diagnostics.Playground.Features.Scenarios;

public sealed class ManagedCrashScenarioRunner : ICrashScenarioRunner
{
    private static readonly HashSet<string> SupportedScenarios =
    [
        "managed-ui-unhandled",
        "managed-background-unhandled",
        "runtime-failfast",
        "runtime-stack-overflow",
        "vendor-handled-exception"
    ];

    public bool CanRun(CrashScenarioDescriptor scenario)
        => SupportedScenarios.Contains(scenario.Key);

    public Task RunAsync(CrashScenarioDescriptor scenario)
    {
        return scenario.Key switch
        {
            "managed-ui-unhandled" => ThrowUnhandledOnCurrentThread(),
            "managed-background-unhandled" => ThrowUnhandledOnBackgroundThread(),
            "runtime-failfast" => FailFast(),
            "runtime-stack-overflow" => OverflowStack(),
            "vendor-handled-exception" => CaptureHandledException(),
            _ => throw new NotSupportedException($"Scenario '{scenario.Key}' is not wired to a crash runner yet.")
        };
    }

    private static Task ThrowUnhandledOnCurrentThread()
        => throw new InvalidOperationException("Simulated unhandled managed exception on the UI thread.");

    private static Task ThrowUnhandledOnBackgroundThread()
    {
        var thread = new Thread(() =>
        {
            throw new InvalidOperationException("Simulated unhandled managed exception on a background thread.");
        })
        {
            IsBackground = true,
            Name = "CrashGallery.UnhandledBackground"
        };

        thread.Start();
        return Task.CompletedTask;
    }

    private static Task FailFast()
    {
        Environment.FailFast("Simulated Environment.FailFast from the MAUI diagnostics playground.");
        return Task.CompletedTask;
    }

    private static Task OverflowStack()
    {
        RecurseUntilStackOverflow(0);
        return Task.CompletedTask;
    }

    private static Task CaptureHandledException()
    {
        try
        {
            throw new InvalidOperationException("Simulated handled exception for vendor capture.");
        }
        catch (InvalidOperationException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }

        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int RecurseUntilStackOverflow(int depth)
    {
        var buffer = new byte[256];
        buffer[0] = (byte)(depth & 0xFF);
        return buffer[0] + RecurseUntilStackOverflow(depth + 1);
    }
}
