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
        "native-apple-abort",
        "native-android-sigsegv",
        "mixed-managed-native",
        "vendor-handled-exception",
        "lifecycle-startup-crash"
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
            "native-apple-abort" => NativeAbort(),
            "native-android-sigsegv" => NativeSegmentationFault(),
            "mixed-managed-native" => MixedManagedNativeCrash(),
            "vendor-handled-exception" => CaptureHandledException(),
            "lifecycle-startup-crash" => ArmStartupCrash(),
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

    private static Task NativeAbort()
    {
        NativeCrashInterop.Abort();
        return Task.CompletedTask;
    }

    private static Task NativeSegmentationFault()
    {
        NativeCrashInterop.RaiseSegmentationFault();
        return Task.CompletedTask;
    }

    private static Task MixedManagedNativeCrash()
    {
        NativeCrashInterop.CrashThroughManagedNativeBoundary();
        return Task.CompletedTask;
    }

    private static async Task ArmStartupCrash()
    {
        StartupCrashCoordinator.Arm();
        await Shell.Current.DisplayAlertAsync(
            "Startup crash armed",
            "Force quit and relaunch the app. The next launch will throw before the first Shell page is created, then automatically clear the startup crash flag.",
            "OK");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int RecurseUntilStackOverflow(int depth)
    {
        var buffer = new byte[256];
        buffer[0] = (byte)(depth & 0xFF);
        return buffer[0] + RecurseUntilStackOverflow(depth + 1);
    }
}
