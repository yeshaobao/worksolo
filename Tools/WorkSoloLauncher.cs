using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

internal static class WorkSoloLauncher
{
    [STAThread]
    private static void Main()
    {
        // 根目录下的启动器，方便像普通软件一样直接双击 WorkSolo.exe。
        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var targetPath = Path.Combine(rootDirectory, "AppLive", "WorkSolo.exe");

        if (!File.Exists(targetPath))
        {
            MessageBox.Show(
                string.Format("未找到主程序：{0}", targetPath),
                "WorkSolo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = targetPath,
            WorkingDirectory = Path.GetDirectoryName(targetPath) ?? rootDirectory,
            UseShellExecute = true
        };

        Process.Start(startInfo);
    }
}
