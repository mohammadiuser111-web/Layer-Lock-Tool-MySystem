using System.ComponentModel;
using LayerLockTool.Interop;

namespace LayerLockTool.Core;

internal sealed class WindowLockManager : IDisposable
{
    private readonly Dictionary<nint, LockedWindow> _lockedWindows = [];
    private readonly System.Windows.Forms.Timer _maintenanceTimer;
    private bool _disposed;

    public WindowLockManager()
    {
        _maintenanceTimer = new System.Windows.Forms.Timer
        {
            Interval = 1200,
            Enabled = true,
        };
        _maintenanceTimer.Tick += OnMaintenanceTick;
    }

    public event EventHandler? Changed;

    public event EventHandler<Exception>? OperationFailed;

    public int Count => _lockedWindows.Count;

    public bool IsLocked(nint window)
    {
        nint target = NativeHelpers.NormalizeTopLevelWindow(window);
        return target != nint.Zero && _lockedWindows.ContainsKey(target);
    }

    public IReadOnlyList<LockedWindow> GetSnapshot() =>
        _lockedWindows.Values
            .Where(item => NativeMethods.IsWindow(item.Handle))
            .OrderByDescending(item => item.LockedAt)
            .ToArray();

    public bool Toggle(nint window)
    {
        nint target = NativeHelpers.NormalizeTopLevelWindow(window);
        if (!CanManage(target))
        {
            return false;
        }

        return _lockedWindows.ContainsKey(target) ? Unlock(target) : Lock(target);
    }

    public bool Lock(nint window)
    {
        nint target = NativeHelpers.NormalizeTopLevelWindow(window);
        if (!CanManage(target) || _lockedWindows.ContainsKey(target))
        {
            return false;
        }

        bool wasTopMost = NativeHelpers.IsTopMost(target);
        if (!SetTopMost(target, enabled: true))
        {
            ReportLastError("Unable to lock the selected window.");
            return false;
        }

        _lockedWindows[target] = new LockedWindow(
            target,
            NativeHelpers.GetWindowTitle(target),
            wasTopMost,
            DateTimeOffset.UtcNow);
        OnChanged();
        return true;
    }

    public bool Unlock(nint window)
    {
        nint target = NativeHelpers.NormalizeTopLevelWindow(window);
        if (!_lockedWindows.Remove(target, out LockedWindow? state))
        {
            return false;
        }

        bool restored = !NativeMethods.IsWindow(target) || SetTopMost(target, state.WasTopMost);
        if (!restored)
        {
            ReportLastError("Unable to restore the selected window.");
        }

        OnChanged();
        return restored;
    }

    public void UnlockAll()
    {
        LockedWindow[] snapshot = _lockedWindows.Values.ToArray();
        _lockedWindows.Clear();

        foreach (LockedWindow state in snapshot)
        {
            if (NativeMethods.IsWindow(state.Handle))
            {
                _ = SetTopMost(state.Handle, state.WasTopMost);
            }
        }

        OnChanged();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _maintenanceTimer.Stop();
        _maintenanceTimer.Tick -= OnMaintenanceTick;
        _maintenanceTimer.Dispose();
        UnlockAll();
    }

    private static bool CanManage(nint window) =>
        window != nint.Zero &&
        NativeMethods.IsWindow(window) &&
        NativeMethods.IsWindowVisible(window);

    private static bool SetTopMost(nint window, bool enabled)
    {
        nint insertAfter = enabled ? NativeConstants.WindowTopMost : NativeConstants.WindowNotTopMost;
        const uint flags = NativeConstants.SwpNoMove |
                           NativeConstants.SwpNoSize |
                           NativeConstants.SwpNoActivate |
                           NativeConstants.SwpAsyncWindowPos;

        return NativeMethods.SetWindowPos(window, insertAfter, 0, 0, 0, 0, flags);
    }

    private void OnMaintenanceTick(object? sender, EventArgs eventArgs)
    {
        bool changed = false;

        foreach (LockedWindow state in _lockedWindows.Values.ToArray())
        {
            if (!NativeMethods.IsWindow(state.Handle))
            {
                _lockedWindows.Remove(state.Handle);
                changed = true;
                continue;
            }

            if (!NativeHelpers.IsTopMost(state.Handle))
            {
                _ = SetTopMost(state.Handle, enabled: true);
            }
        }

        if (changed)
        {
            OnChanged();
        }
    }

    private void ReportLastError(string message)
    {
        int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        OperationFailed?.Invoke(this, new Win32Exception(error, message));
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
