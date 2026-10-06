using System.Runtime.InteropServices;

namespace LayerLockTool.Interop;

internal static partial class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetWindowsHookEx(
        int hookId,
        HookProcedure hookProcedure,
        nint module,
        uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(nint hook);

    [DllImport("user32.dll")]
    internal static extern nint CallNextHookEx(nint hook, int code, nuint message, nint data);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern nint GetModuleHandle(string? moduleName);

    [DllImport("user32.dll")]
    internal static extern nint WindowFromPoint(NativePoint point);

    [DllImport("user32.dll")]
    internal static extern nint GetAncestor(nint window, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindow(nint window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindowVisible(nint window);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint GetWindowLongPtr(nint window, int index);

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW")]
    internal static unsafe partial int GetClassName(nint window, char* className, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(nint window, out uint processId);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextW")]
    internal static unsafe partial int GetWindowText(nint window, char* text, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SendMessageTimeout(
        nint window,
        uint message,
        nuint wParam,
        nint lParam,
        uint flags,
        uint timeout,
        out nuint result);

    [DllImport("user32.dll")]
    internal static extern nint GetSystemMenu(nint window, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool InsertMenuItem(
        nint menu,
        uint item,
        [MarshalAs(UnmanagedType.Bool)] bool byPosition,
        ref MenuItemInfo menuItemInfo);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RemoveMenu(nint menu, uint position, uint flags);

    [DllImport("user32.dll")]
    internal static extern uint GetMenuState(nint menu, uint item, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint module,
        WinEventProcedure callback,
        uint processId,
        uint threadId,
        uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWinEvent(nint hook);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetWindowPos(
        nint window,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll")]
    internal static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetForegroundWindow(nint window);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RegisterHotKey(nint window, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterHotKey(nint window, int id);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AppendMenu(nint menu, uint flags, nuint identifier, string? text);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DestroyMenu(nint menu);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint TrackPopupMenuEx(
        nint menu,
        uint flags,
        int x,
        int y,
        nint window,
        nint parameters);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PostMessage(nint window, uint message, nuint wParam, nint lParam);
}
