using LayerLockTool.Interop;

namespace LayerLockTool.UI;

internal sealed class GlobalHotKey : IDisposable
{
    private readonly nint _window;
    private bool _registered;

    public GlobalHotKey(nint window)
    {
        _window = window;
        _registered = NativeMethods.RegisterHotKey(
            window,
            NativeConstants.HotKeyId,
            NativeConstants.ModWin | NativeConstants.ModControl | NativeConstants.ModNoRepeat,
            NativeConstants.VirtualKeySpace);
    }

    public bool IsRegistered => _registered;

    public void Dispose()
    {
        if (_registered)
        {
            _ = NativeMethods.UnregisterHotKey(_window, NativeConstants.HotKeyId);
            _registered = false;
        }
    }
}
