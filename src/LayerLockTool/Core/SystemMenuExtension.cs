using System.ComponentModel;
using System.Runtime.InteropServices;
using LayerLockTool.Interop;
using LayerLockTool.UI;

namespace LayerLockTool.Core;

internal sealed class SystemMenuExtension : IDisposable
{
    private const uint FirstCommandId = 0x5A11;
    private const uint LastCommandId = 0x5AFF;

    private readonly MessageWindow _messageWindow;
    private readonly WindowLockManager _lockManager;
    private readonly HookProcedure _mouseProcedure;
    private readonly WinEventProcedure _menuEventProcedure;
    private readonly WinEventProcedure _invokeEventProcedure;
    private nint _mouseHook;
    private nint _menuEventHook;
    private nint _invokeEventHook;
    private ActiveMenu? _activeMenu;
    private bool _disposed;

    public SystemMenuExtension(MessageWindow messageWindow, WindowLockManager lockManager)
    {
        _messageWindow = messageWindow;
        _lockManager = lockManager;
        _mouseProcedure = OnMouseHook;
        _menuEventProcedure = OnMenuEvent;
        _invokeEventProcedure = OnInvokeEvent;

        InstallHooks();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RemoveActiveItem();

        UninstallHooks();
    }

    private void InstallHooks()
    {
        nint module = NativeMethods.GetModuleHandle(null);
        _mouseHook = NativeMethods.SetWindowsHookEx(
            NativeConstants.WhMouseLowLevel,
            _mouseProcedure,
            module,
            0);

        _menuEventHook = NativeMethods.SetWinEventHook(
            NativeConstants.EventSystemMenuEnd,
            NativeConstants.EventSystemMenuEnd,
            nint.Zero,
            _menuEventProcedure,
            0,
            0,
            NativeConstants.WinEventOutOfContext | NativeConstants.WinEventSkipOwnProcess);

        _invokeEventHook = NativeMethods.SetWinEventHook(
            NativeConstants.EventObjectInvoked,
            NativeConstants.EventObjectInvoked,
            nint.Zero,
            _invokeEventProcedure,
            0,
            0,
            NativeConstants.WinEventOutOfContext | NativeConstants.WinEventSkipOwnProcess);

        if (_mouseHook == nint.Zero || _menuEventHook == nint.Zero || _invokeEventHook == nint.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            UninstallHooks();
            throw new Win32Exception(error, "Unable to install Windows event hooks.");
        }
    }

    private void UninstallHooks()
    {
        if (_mouseHook != nint.Zero)
        {
            _ = NativeMethods.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = nint.Zero;
        }

        if (_menuEventHook != nint.Zero)
        {
            _ = NativeMethods.UnhookWinEvent(_menuEventHook);
            _menuEventHook = nint.Zero;
        }

        if (_invokeEventHook != nint.Zero)
        {
            _ = NativeMethods.UnhookWinEvent(_invokeEventHook);
            _invokeEventHook = nint.Zero;
        }
    }

    private nint OnMouseHook(int code, nuint message, nint data)
    {
        if (code >= 0 && message == NativeConstants.WmRightButtonUp)
        {
            LowLevelMouseData mouseData = Marshal.PtrToStructure<LowLevelMouseData>(data);
            nint target = WindowLocator.FindCaptionWindow(mouseData.Point);
            if (target != nint.Zero)
            {
                AddTemporaryMenuItem(target);
            }
        }

        return NativeMethods.CallNextHookEx(_mouseHook, code, message, data);
    }

    private void OnMenuEvent(
        nint hook,
        uint eventType,
        nint window,
        int objectId,
        int childId,
        uint eventThread,
        uint eventTime)
    {
        _ = hook;
        _ = window;
        _ = objectId;
        _ = childId;
        _ = eventThread;
        _ = eventTime;

        if (_activeMenu is null)
        {
            return;
        }

        if (eventType == NativeConstants.EventSystemMenuEnd)
        {
            RemoveActiveItem();
        }
    }

    private void OnInvokeEvent(
        nint hook,
        uint eventType,
        nint window,
        int objectId,
        int childId,
        uint eventThread,
        uint eventTime)
    {
        _ = hook;
        _ = eventType;
        _ = window;
        _ = eventThread;
        _ = eventTime;

        ActiveMenu? active = _activeMenu;
        if (active is not null &&
            objectId == NativeConstants.ObjectIdSystemMenu &&
            childId == active.CommandId)
        {
            _messageWindow.PostToggle(active.Window);
        }
    }

    private void AddTemporaryMenuItem(nint window)
    {
        RemoveActiveItem();

        nint menu = NativeMethods.GetSystemMenu(window, revert: false);
        if (menu == nint.Zero)
        {
            return;
        }

        uint commandId = FindAvailableCommandId(menu);
        if (commandId == 0)
        {
            return;
        }

        var item = MenuItemInfo.Create();
        item.Mask = NativeConstants.MenuItemId |
                    NativeConstants.MenuItemState |
                    NativeConstants.MenuItemString;
        item.Id = commandId;
        item.State = _lockManager.IsLocked(window)
            ? NativeConstants.MenuStateChecked
            : NativeConstants.MenuStateEnabled;
        item.TypeData = AppStrings.LockLayer;
        item.TextLength = (uint)item.TypeData.Length;

        if (NativeMethods.InsertMenuItem(menu, 0, byPosition: true, ref item))
        {
            _activeMenu = new ActiveMenu(window, menu, unchecked((int)commandId));
        }
    }

    private static uint FindAvailableCommandId(nint menu)
    {
        for (uint commandId = FirstCommandId; commandId <= LastCommandId; commandId++)
        {
            if (NativeMethods.GetMenuState(menu, commandId, NativeConstants.MenuByCommand) == uint.MaxValue)
            {
                return commandId;
            }
        }

        return 0;
    }

    private void RemoveActiveItem()
    {
        ActiveMenu? active = _activeMenu;
        _activeMenu = null;

        if (active is not null && NativeMethods.IsWindow(active.Window))
        {
            _ = NativeMethods.RemoveMenu(
                active.Menu,
                unchecked((uint)active.CommandId),
                NativeConstants.MenuByCommand);
        }
    }

    private sealed record ActiveMenu(nint Window, nint Menu, int CommandId);
}
