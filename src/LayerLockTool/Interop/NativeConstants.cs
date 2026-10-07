namespace LayerLockTool.Interop;

internal static class NativeConstants
{
    public const int WhKeyboardLowLevel = 13;
    public const int WhMouseLowLevel = 14;
    public const uint WmKeyDown = 0x0100;
    public const uint WmSystemKeyDown = 0x0104;
    public const uint WmLeftButtonUp = 0x0202;
    public const uint WmRightButtonUp = 0x0205;
    public const uint WmHotKey = 0x0312;
    public const uint WmNull = 0x0000;
    public const uint WmNcHitTest = 0x0084;
    public const int HitCaption = 2;
    public const int HitSystemMenu = 3;
    public const uint GaRoot = 2;
    public const int GwlStyle = -16;
    public const int GwlExStyle = -20;
    public const long WsSystemMenu = 0x00080000L;
    public const long WsExTopMost = 0x00000008L;

    public const uint SmtoAbortIfHung = 0x0002;
    public const uint SmtoBlock = 0x0001;

    public const uint EventSystemMenuEnd = 0x0005;
    public const uint EventObjectInvoked = 0x8013;
    public const int ObjectIdSystemMenu = -1;
    public const uint WinEventOutOfContext = 0x0000;
    public const uint WinEventSkipOwnProcess = 0x0002;

    public const uint MenuItemId = 0x00000002;
    public const uint MenuItemState = 0x00000001;
    public const uint MenuItemString = 0x00000040;
    public const uint MenuStateChecked = 0x00000008;
    public const uint MenuStateEnabled = 0x00000000;
    public const uint MenuStateHighlighted = 0x00000080;
    public const uint MenuByCommand = 0x00000000;
    public const uint MenuByPosition = 0x00000400;
    public const uint MenuString = 0x00000000;
    public const uint MenuSeparator = 0x00000800;
    public const uint MenuDisabled = 0x00000002;
    public const uint MenuGrayed = 0x00000001;
    public const uint MenuPopup = 0x00000010;
    public const uint MenuChecked = 0x00000008;

    public const uint SwpNoSize = 0x0001;
    public const uint SwpNoMove = 0x0002;
    public const uint SwpNoActivate = 0x0010;
    public const uint SwpAsyncWindowPos = 0x4000;
    public static readonly nint WindowTopMost = new(-1);
    public static readonly nint WindowNotTopMost = new(-2);

    public const uint ModControl = 0x0002;
    public const uint ModWin = 0x0008;
    public const uint ModNoRepeat = 0x4000;
    public const uint VirtualKeyEnter = 0x0D;
    public const uint VirtualKeySpace = 0x20;

    public const uint TrackRightButton = 0x0002;
    public const uint TrackBottomAlign = 0x0020;
    public const uint TrackReturnCommand = 0x0100;

    public const int WsExToolWindow = 0x00000080;
    public const int WsExNoActivate = 0x08000000;
    public const int WsPopup = unchecked((int)0x80000000);

    public const int HotKeyId = 0x4C4C;
    public const int ToggleActiveCommand = 1001;
    public const int UnlockAllCommand = 1002;
    public const int StartupCommand = 1003;
    public const int ExitCommand = 1004;
    public const int LockedWindowCommandBase = 2000;

    public const int MaxWindowTitleLength = 180;
}
