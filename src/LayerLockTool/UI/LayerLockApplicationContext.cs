using LayerLockTool.Core;

namespace LayerLockTool.UI;

internal sealed class LayerLockApplicationContext : ApplicationContext, IDisposable
{
    private readonly MessageWindow _messageWindow;
    private readonly WindowLockManager _lockManager;
    private readonly SystemMenuExtension _systemMenuExtension;
    private readonly GlobalHotKey _globalHotKey;
    private readonly TrayIconController _trayIcon;
    private bool _disposed;

    public LayerLockApplicationContext()
    {
        _messageWindow = new MessageWindow();
        _lockManager = new WindowLockManager();
        _trayIcon = new TrayIconController(_messageWindow, _lockManager, ExitThread);
        _globalHotKey = new GlobalHotKey(_messageWindow.Handle);
        _systemMenuExtension = new SystemMenuExtension(_messageWindow, _lockManager);

        _messageWindow.ToggleRequested += OnToggleRequested;
        _messageWindow.HotKeyPressed += OnHotKeyPressed;
        _lockManager.OperationFailed += OnOperationFailed;

        if (!_globalHotKey.IsRegistered)
        {
            _trayIcon.ShowError(AppStrings.HotKeyUnavailable);
        }
    }

    void IDisposable.Dispose() => Dispose(disposing: true);

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            base.Dispose(disposing);
            return;
        }

        _disposed = true;
        if (disposing)
        {
            _messageWindow.ToggleRequested -= OnToggleRequested;
            _messageWindow.HotKeyPressed -= OnHotKeyPressed;
            _lockManager.OperationFailed -= OnOperationFailed;
            _systemMenuExtension.Dispose();
            _globalHotKey.Dispose();
            _lockManager.Dispose();
            _trayIcon.Dispose();
            _messageWindow.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnToggleRequested(object? sender, nint window)
    {
        if (_lockManager.Toggle(window))
        {
            _trayIcon.ShowStateChanged(window);
        }
    }

    private void OnHotKeyPressed(object? sender, EventArgs eventArgs)
    {
        nint target = WindowLocator.GetForegroundManageableWindow();
        if (target != nint.Zero && _lockManager.Toggle(target))
        {
            _trayIcon.ShowStateChanged(target);
        }
    }

    private void OnOperationFailed(object? sender, Exception exception)
    {
        _ = exception;
        _trayIcon.ShowError(AppStrings.OperationFailed);
    }
}
