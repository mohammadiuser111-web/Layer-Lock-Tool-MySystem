using LayerLockTool.Interop;

namespace LayerLockTool.Core;

internal static class WindowLocator
{
    private static readonly HashSet<string> ShellWindowClasses = new(StringComparer.Ordinal)
    {
        "Progman",
        "WorkerW",
        "Shell_TrayWnd",
        "Shell_SecondaryTrayWnd",
        "#32768",
    };

    public static nint FindCaptionWindow(NativePoint point)
    {
        nint candidate = NativeHelpers.NormalizeTopLevelWindow(NativeMethods.WindowFromPoint(point));
        if (!IsEligible(candidate))
        {
            return nint.Zero;
        }

        nuint result;
        nint sent = NativeMethods.SendMessageTimeout(
            candidate,
            NativeConstants.WmNcHitTest,
            0,
            NativeHelpers.PackPoint(point),
            NativeConstants.SmtoAbortIfHung | NativeConstants.SmtoBlock,
            75,
            out result);

        if (sent == nint.Zero)
        {
            return nint.Zero;
        }

        int hitTest = unchecked((int)result);
        return hitTest is NativeConstants.HitCaption or NativeConstants.HitSystemMenu
            ? candidate
            : nint.Zero;
    }

    public static nint GetForegroundManageableWindow()
    {
        nint candidate = NativeHelpers.NormalizeTopLevelWindow(NativeMethods.GetForegroundWindow());
        return IsEligible(candidate) ? candidate : nint.Zero;
    }

    private static unsafe bool IsEligible(nint window)
    {
        if (window == nint.Zero ||
            !NativeMethods.IsWindow(window) ||
            !NativeMethods.IsWindowVisible(window))
        {
            return false;
        }

        _ = NativeMethods.GetWindowThreadProcessId(window, out uint processId);
        if (processId == (uint)Environment.ProcessId)
        {
            return false;
        }

        long style = NativeMethods.GetWindowLongPtr(window, NativeConstants.GwlStyle).ToInt64();
        if ((style & NativeConstants.WsSystemMenu) == 0)
        {
            return false;
        }

        Span<char> classNameBuffer = stackalloc char[96];
        int classNameLength;
        fixed (char* pointer = classNameBuffer)
        {
            classNameLength = NativeMethods.GetClassName(window, pointer, classNameBuffer.Length);
        }

        string className = classNameLength > 0
            ? new string(classNameBuffer[..classNameLength])
            : string.Empty;
        return !ShellWindowClasses.Contains(className) && processId != (uint)Environment.ProcessId;
    }
}
