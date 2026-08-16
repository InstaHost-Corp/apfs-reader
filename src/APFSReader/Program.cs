using System;
using System.Windows.Forms;

namespace APFSReader
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Has to run before the first window is created, or the shell has already decided
            // this process gets light chrome.
            DarkMode.Init();

            Application.Run(new MainForm());
        }
    }
}
