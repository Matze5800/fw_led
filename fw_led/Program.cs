using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace fw_led
{
    internal static class Program
    {
        [DllImport("fw_dll", EntryPoint = "cmd_led")]
        static extern void cmd_led(int state);

        [STAThread]
        static void Main()
        {
            if (!Program.IsAdministrator())
            {
                var exeName = Environment.ProcessPath;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                startInfo.Arguments = "restart";
                Process.Start(startInfo);
                Application.Exit();
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            ApplicationConfiguration.Initialize();
            Application.Run(new fw_led());
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            cmd_led(4);
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}