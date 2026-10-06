using System.Globalization;

namespace LayerLockTool;

internal static class AppStrings
{
    private static bool IsPersian => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fa";

    public const string ProductName = "Layer Lock Tool - MySystem";

    public static string LockLayer => IsPersian ? "قفل لایه (همیشه بالا)" : "Lock layer (always on top)";

    public static string LockActiveWindow => IsPersian ? "قفل پنجرهٔ فعال" : "Lock active window";

    public static string UnlockActiveWindow => IsPersian ? "باز کردن قفل پنجرهٔ فعال" : "Unlock active window";

    public static string LockedWindows => IsPersian ? "پنجره‌های قفل‌شده" : "Locked windows";

    public static string NoLockedWindows => IsPersian ? "هیچ پنجره‌ای قفل نیست" : "No locked windows";

    public static string UnlockAll => IsPersian ? "باز کردن همهٔ قفل‌ها" : "Unlock all";

    public static string StartWithWindows => IsPersian ? "اجرا همراه ویندوز" : "Start with Windows";

    public static string Exit => IsPersian ? "خروج" : "Exit";

    public static string AlreadyRunning => IsPersian ? "برنامه از قبل در حال اجراست." : "Layer Lock Tool is already running.";

    public static string Locked(string title) => IsPersian ? $"«{title}» قفل شد." : $"“{title}” is locked.";

    public static string Unlocked(string title) => IsPersian ? $"قفل «{title}» باز شد." : $"“{title}” is unlocked.";

    public static string OperationFailed => IsPersian ? "تغییر وضعیت این پنجره ممکن نیست. برنامه را با دسترسی Administrator اجرا کنید." : "This window could not be changed. Try running the tool as Administrator.";

    public static string HotKeyUnavailable => IsPersian ? "میان‌بر Win + Ctrl + Space در اختیار برنامهٔ دیگری است." : "Win + Ctrl + Space is already used by another app.";
}
