namespace LayerLockTool.Interop;

internal static class NativeHelpers
{
    public static nint PackPoint(NativePoint point)
    {
        uint packed = (ushort)point.X | ((uint)(ushort)point.Y << 16);
        return unchecked((nint)(int)packed);
    }

    public static bool IsTopMost(nint window)
    {
        long extendedStyle = NativeMethods.GetWindowLongPtr(window, NativeConstants.GwlExStyle).ToInt64();
        return (extendedStyle & NativeConstants.WsExTopMost) != 0;
    }

    public static nint NormalizeTopLevelWindow(nint window)
    {
        if (window == nint.Zero)
        {
            return nint.Zero;
        }

        nint root = NativeMethods.GetAncestor(window, NativeConstants.GaRoot);
        return root == nint.Zero ? window : root;
    }

    public static unsafe string GetWindowTitle(nint window)
    {
        Span<char> buffer = stackalloc char[NativeConstants.MaxWindowTitleLength];
        int length;
        fixed (char* pointer = buffer)
        {
            length = NativeMethods.GetWindowText(window, pointer, buffer.Length);
        }

        string value = length > 0 ? new string(buffer[..length]).Trim() : string.Empty;
        return string.IsNullOrWhiteSpace(value) ? "Window" : value;
    }

    public static string TrimMenuText(string value, int maxLength = 64)
    {
        string singleLine = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return singleLine.Length <= maxLength ? singleLine : string.Concat(singleLine.AsSpan(0, maxLength - 1), "…");
    }
}
