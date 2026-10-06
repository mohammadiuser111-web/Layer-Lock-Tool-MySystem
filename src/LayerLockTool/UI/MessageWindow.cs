using LayerLockTool.Interop;

namespace LayerLockTool.UI;

internal sealed class MessageWindow : NativeWindow, IDisposable
{
    private const int ToggleMessage = 0x8000 + 0x41;
    private bool _disposed;

    public MessageWindow()
    {
        CreateHandle(new CreateParams
        {
            Caption = AppStrings.ProductName,
            X = -32_000,
            Y = -32_000,
            Width = 1,
            Height = 1,
            Style = NativeConstants.WsPopup,
            ExStyle = NativeConstants.WsExToolWindow | NativeConstants.WsExNoActivate,
        });
    }

    public event EventHandler<nint>? ToggleRequested;

    public event EventHandler? HotKeyPressed;

    public void PostToggle(nint window)
    {
        if (window != nint.Zero)
        {
            _ = NativeMethods.PostMessage(Handle, ToggleMessage, unchecked((nuint)window), nint.Zero);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DestroyHandle();
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == ToggleMessage)
        {
            ToggleRequested?.Invoke(this, message.WParam);
            return;
        }

        if (message.Msg == NativeConstants.WmHotKey && message.WParam.ToInt32() == NativeConstants.HotKeyId)
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
            return;
        }

        base.WndProc(ref message);
    }
}
