using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Maui.Diagnostics.Playground.Features.Scenarios;

internal static partial class NativeCrashInterop
{
    private const int SigIll = 4;
    private const int SigSegv = 11;

    public static void Abort()
    {
        AbortNative();
    }

    public static void RaiseSegmentationFault()
    {
        RaiseNative(SigSegv);
    }

    public static void RaiseIllegalInstruction()
    {
        RaiseNative(SigIll);
    }

    public static void CrashOnBackgroundThread()
    {
        var thread = new Thread(RaiseSegmentationFault)
        {
            IsBackground = true,
            Name = "CrashGallery.NativeBackground"
        };

        thread.Start();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CrashThroughManagedNativeBoundary()
    {
        ManagedFrameOne();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ManagedFrameOne()
    {
        ManagedFrameTwo();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ManagedFrameTwo()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            RaiseSegmentationFault();
            return;
        }

        Abort();
    }

#if IOS || MACCATALYST
    [DllImport("__Internal", EntryPoint = "abort")]
    private static extern void AbortNative();

    [DllImport("__Internal", EntryPoint = "raise")]
    private static extern int RaiseNative(int signal);
#elif ANDROID
    [DllImport("libc", EntryPoint = "abort")]
    private static extern void AbortNative();

    [DllImport("libc", EntryPoint = "raise")]
    private static extern int RaiseNative(int signal);
#else
    private static void AbortNative()
    {
        Environment.FailFast("Native abort is only implemented for Android, iOS, and Mac Catalyst.");
    }

    private static int RaiseNative(int signal)
    {
        Environment.FailFast($"Native signal {signal} is only implemented for Android, iOS, and Mac Catalyst.");
        return 0;
    }
#endif
}
