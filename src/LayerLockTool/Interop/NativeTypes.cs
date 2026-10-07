using System.Runtime.InteropServices;

namespace LayerLockTool.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LowLevelMouseData
{
    public NativePoint Point;
    public uint MouseData;
    public uint Flags;
    public uint Time;
    public nuint ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LowLevelKeyboardData
{
    public uint VirtualKeyCode;
    public uint ScanCode;
    public uint Flags;
    public uint Time;
    public nuint ExtraInfo;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MenuItemInfo
{
    public uint Size;
    public uint Mask;
    public uint Type;
    public uint State;
    public uint Id;
    public nint SubMenu;
    public nint CheckedBitmap;
    public nint UncheckedBitmap;
    public nuint ItemData;

    [MarshalAs(UnmanagedType.LPWStr)]
    public string? TypeData;

    public uint TextLength;
    public nint ItemBitmap;

    public static MenuItemInfo Create() => new()
    {
        Size = (uint)Marshal.SizeOf<MenuItemInfo>(),
    };
}

internal delegate nint HookProcedure(int code, nuint message, nint data);

internal delegate void WinEventProcedure(
    nint hook,
    uint eventType,
    nint window,
    int objectId,
    int childId,
    uint eventThread,
    uint eventTime);
