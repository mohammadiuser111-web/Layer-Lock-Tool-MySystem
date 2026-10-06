using System.Reflection;
using LayerLockTool.Core;
using LayerLockTool.Infrastructure;
using LayerLockTool.Interop;

namespace LayerLockTool.UI;

internal sealed class TrayIconController : IDisposable
{
    private readonly MessageWindow _messageWindow;
    private readonly WindowLockManager _lockManager;
    private readonly Action _exitAction;
    private readonly NotifyIcon _notifyIcon;
    private nint _lastForegroundWindow;
    private bool _disposed;

    public TrayIconController(
        MessageWindow messageWindow,
        WindowLockManager lockManager,
        Action exitAction)
    {
        _messageWindow = messageWindow;
        _lockManager = lockManager;
        _exitAction = exitAction;
        _notifyIcon = new NotifyIcon
        {
            Icon = LoadIcon(),
            Text = AppStrings.ProductName,
            Visible = true,
        };

        _notifyIcon.MouseDown += OnMouseDown;
        _notifyIcon.MouseUp += OnMouseUp;
        _notifyIcon.DoubleClick += OnDoubleClick;
    }

    public void ShowStateChanged(nint window)
    {
        if (!NativeMethods.IsWindow(window))
        {
            return;
        }

        string title = NativeHelpers.TrimMenuText(NativeHelpers.GetWindowTitle(window), 52);
        string message = _lockManager.IsLocked(window)
            ? AppStrings.Locked(title)
            : AppStrings.Unlocked(title);

        _notifyIcon.ShowBalloonTip(
            timeout: 1200,
            tipTitle: AppStrings.ProductName,
            tipText: message,
            tipIcon: ToolTipIcon.None);
    }

    public void ShowError(string message)
    {
        _notifyIcon.ShowBalloonTip(
            timeout: 2500,
            tipTitle: AppStrings.ProductName,
            tipText: message,
            tipIcon: ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.MouseDown -= OnMouseDown;
        _notifyIcon.MouseUp -= OnMouseUp;
        _notifyIcon.DoubleClick -= OnDoubleClick;
        _notifyIcon.Icon?.Dispose();
        _notifyIcon.Dispose();
    }

    private static Icon LoadIcon()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LayerLockTool.App.ico");
        if (stream is null)
        {
            return SystemIcons.Application;
        }

        using var icon = new Icon(stream);
        return (Icon)icon.Clone();
    }

    private void OnMouseDown(object? sender, MouseEventArgs eventArgs)
    {
        _lastForegroundWindow = WindowLocator.GetForegroundManageableWindow();
    }

    private void OnMouseUp(object? sender, MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButtons.Right)
        {
            ShowNativeMenu();
        }
    }

    private void OnDoubleClick(object? sender, EventArgs eventArgs)
    {
        ToggleLastForegroundWindow();
    }

    private void ShowNativeMenu()
    {
        nint menu = NativeMethods.CreatePopupMenu();
        if (menu == nint.Zero)
        {
            return;
        }

        var windowCommands = new Dictionary<uint, nint>();
        try
        {
            BuildMenu(menu, windowCommands);
            if (!NativeMethods.GetCursorPos(out NativePoint point))
            {
                return;
            }

            _ = NativeMethods.SetForegroundWindow(_messageWindow.Handle);
            uint command = NativeMethods.TrackPopupMenuEx(
                menu,
                NativeConstants.TrackRightButton |
                NativeConstants.TrackBottomAlign |
                NativeConstants.TrackReturnCommand,
                point.X,
                point.Y,
                _messageWindow.Handle,
                nint.Zero);
            _ = NativeMethods.PostMessage(_messageWindow.Handle, NativeConstants.WmNull, 0, nint.Zero);

            ExecuteCommand(command, windowCommands);
        }
        finally
        {
            _ = NativeMethods.DestroyMenu(menu);
        }
    }

    private void BuildMenu(nint menu, IDictionary<uint, nint> windowCommands)
    {
        bool activeLocked = _lastForegroundWindow != nint.Zero && _lockManager.IsLocked(_lastForegroundWindow);
        string toggleText = activeLocked ? AppStrings.UnlockActiveWindow : AppStrings.LockActiveWindow;
        _ = NativeMethods.AppendMenu(
            menu,
            NativeConstants.MenuString,
            NativeConstants.ToggleActiveCommand,
            $"{toggleText}\tWin+Ctrl+Space");

        IReadOnlyList<LockedWindow> lockedWindows = _lockManager.GetSnapshot();
        nint windowsSubmenu = NativeMethods.CreatePopupMenu();
        if (windowsSubmenu != nint.Zero)
        {
            if (lockedWindows.Count == 0)
            {
                _ = NativeMethods.AppendMenu(
                    windowsSubmenu,
                    NativeConstants.MenuString | NativeConstants.MenuDisabled | NativeConstants.MenuGrayed,
                    0,
                    AppStrings.NoLockedWindows);
            }
            else
            {
                for (int index = 0; index < lockedWindows.Count; index++)
                {
                    uint command = (uint)(NativeConstants.LockedWindowCommandBase + index);
                    LockedWindow lockedWindow = lockedWindows[index];
                    windowCommands[command] = lockedWindow.Handle;
                    _ = NativeMethods.AppendMenu(
                        windowsSubmenu,
                        NativeConstants.MenuString | NativeConstants.MenuChecked,
                        command,
                        NativeHelpers.TrimMenuText(lockedWindow.Title));
                }
            }

            _ = NativeMethods.AppendMenu(
                menu,
                NativeConstants.MenuPopup,
                unchecked((nuint)windowsSubmenu),
                $"{AppStrings.LockedWindows} ({lockedWindows.Count})");
        }

        uint unlockFlags = NativeConstants.MenuString;
        if (lockedWindows.Count == 0)
        {
            unlockFlags |= NativeConstants.MenuDisabled | NativeConstants.MenuGrayed;
        }

        _ = NativeMethods.AppendMenu(menu, unlockFlags, NativeConstants.UnlockAllCommand, AppStrings.UnlockAll);
        _ = NativeMethods.AppendMenu(menu, NativeConstants.MenuSeparator, 0, null);

        uint startupFlags = NativeConstants.MenuString;
        if (StartupRegistration.IsEnabled())
        {
            startupFlags |= NativeConstants.MenuChecked;
        }

        _ = NativeMethods.AppendMenu(menu, startupFlags, NativeConstants.StartupCommand, AppStrings.StartWithWindows);
        _ = NativeMethods.AppendMenu(menu, NativeConstants.MenuSeparator, 0, null);
        _ = NativeMethods.AppendMenu(menu, NativeConstants.MenuString, NativeConstants.ExitCommand, AppStrings.Exit);
    }

    private void ExecuteCommand(uint command, Dictionary<uint, nint> windowCommands)
    {
        switch (command)
        {
            case NativeConstants.ToggleActiveCommand:
                ToggleLastForegroundWindow();
                break;
            case NativeConstants.UnlockAllCommand:
                _lockManager.UnlockAll();
                break;
            case NativeConstants.StartupCommand:
                StartupRegistration.SetEnabled(!StartupRegistration.IsEnabled());
                break;
            case NativeConstants.ExitCommand:
                _exitAction();
                break;
            default:
                if (windowCommands.TryGetValue(command, out nint window))
                {
                    _ = _lockManager.Unlock(window);
                }

                break;
        }
    }

    private void ToggleLastForegroundWindow()
    {
        nint target = _lastForegroundWindow != nint.Zero
            ? _lastForegroundWindow
            : WindowLocator.GetForegroundManageableWindow();

        if (target != nint.Zero && _lockManager.Toggle(target))
        {
            ShowStateChanged(target);
        }
    }
}
