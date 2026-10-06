using System.Threading;
using LayerLockTool.UI;

namespace LayerLockTool;

internal static class Program
{
    private const string MutexName = "Local\\MySystem.LayerLockTool.38C0BBE8";

    [STAThread]
    private static void Main()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            MessageBox.Show(
                "Layer Lock Tool requires Windows 11.",
                AppStrings.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var singleInstance = new Mutex(initiallyOwned: true, MutexName, out bool isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                AppStrings.AlreadyRunning,
                AppStrings.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        using var context = new LayerLockApplicationContext();
        Application.Run(context);
    }
}
