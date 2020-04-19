using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Timers;
using System.Threading;

namespace PUBG_MAPHACK
{
    public class GameMonitor
    {
        
        private ScreenAnalyzer sAnalyzer = new ScreenAnalyzer();
        private IKeyboardMouseEvents m_GlobalHook;
        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer reviveTimer;
        private int ejectSpeed = 80;
        public Dictionary<string, string[]> playerKeyBindings;

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        public GameMonitor()
        {
            playerKeyBindings = getPlayerKeybindings();

            // Key/Mouse combinations to monitor
            var assignment = new Dictionary<Combination, Action> { };

            // Nade cooking key combiantions from player config
            foreach (var fire in playerKeyBindings["Fire"])
            {
                foreach (var cook in playerKeyBindings["StartCookingThrowable"])
                {
                    string keyCombo = fire + "+" + cook;
                    // Add every combination to monitor
                    assignment.Add(Combination.FromString(keyCombo), suicideByNade);
                }
            }

            // Hold interact button event (to detect if player is reviving someone)
            foreach (var interact in playerKeyBindings["Interact"])
            {
                // Add every combination to monitor
                assignment.Add(Combination.FromString(interact), holdingInteractButton);
            }

            // Install listener
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.OnCombination(assignment);

            // Set timer to check if player is driving
            aTimer = new System.Timers.Timer(3000);
            aTimer.Elapsed += isPlayerDriving;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            // Set timer that is used to trigger player to shoot his teammate when reviving
            reviveTimer = new System.Timers.Timer(7000);
            reviveTimer.Elapsed += fireeeeeeeeeeeeeeeee;
            reviveTimer.AutoReset = false;
            reviveTimer.Enabled = false;
        }

        public void holdingInteractButton()
        {
            void ReleaseInteractEvent(object sender, KeyEventArgs e)
            {
                reviveTimer.Stop();

                // Remove button release listener
                m_GlobalHook.KeyUp -= ReleaseInteractEvent;
            }

            if(reviveTimer.Enabled == false)
            {
                // Set event that triggers when the player releases interact button
                m_GlobalHook.KeyUp += ReleaseInteractEvent;
            }

            // Start the timer to start shooting
            reviveTimer.Enabled = true;
        }

        private void fireeeeeeeeeeeeeeeee(Object source, ElapsedEventArgs e)
        {
            /* Trigger 20 shots by clicking left mouse button 20 times.
               This trigger if the player holds the interact button for 7 seconds */
            for (int i = 0; i < 20; i++)
            {
                // Left mouse down
                mouse_event(0x02, 0, 0, 0, 0);
                Thread.Sleep(50);
                // Left mouse up
                mouse_event(0x04, 0, 0, 0, 0);
            }
            Program.triggerTracker += @"Revive fire triggered | ";

        }

        public void suicideByNade()
        {
            void ReleaseNadeEvent(object sender, MouseEventExtArgs e)
            {
                // Trigger weapon change to drop nade (two different keys just to be safe)
                SendKeys.SendWait("{1}"); // Try switch to main weapon
                SendKeys.SendWait("{X}"); // Try holster weapons
                // Remove mouse up listener
                m_GlobalHook.MouseUpExt -= ReleaseNadeEvent;
                Program.triggerTracker += @"Suicide by nade triggered | ";
            }

            // Set event that triggers when the player releases left mouse down (throw nade)
            m_GlobalHook.MouseUpExt += ReleaseNadeEvent;
        }

        public void suicideByEject()
        {
            // Simple but effective, trigger exit vehicle by simulating exit vehicle keypress
            if(playerKeyBindings.ContainsKey("Interact"))
            {
                SendKeys.SendWait("{" + playerKeyBindings["Interact"][0] + "}");
            }
            SendKeys.SendWait("{F}"); // Extra saftey (default key binding)
            Program.triggerTracker += @"Suicide by eject triggered | ";
        }

        private async void isPlayerDriving(Object source, ElapsedEventArgs e)
        {
            int screenHeight = Int32.Parse(playerKeyBindings["ResolutionSizeY"][0]);

            // Look for km/h in left corner of screen
            string results = await sAnalyzer.Analyze(0, screenHeight - 120, 600, 95);

            Program.debug_log(results);

            if (results.Contains("km/h"))
            {
                // Get speed as number
                int speed = getDrivingSpeed(results);

                Program.debug_log("Player driving at speed: " + speed.ToString());
                
                // if speed is over ejectspeed it's time to SEND IT!!!
                if(speed > ejectSpeed)
                {
                    suicideByEject(); // BYE :)
                }
            }
        }

        private int getDrivingSpeed(string analyzerResults)
        {
            String[] spearator = { "\n" };
            Int32 count = 999;

            // Split results into array
            String[] strlist = analyzerResults.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);

            // Loop through array and find speed
            foreach (String s in strlist)
            {
                if (s.Contains("km/h"))
                {
                    // Results are a bit unpredictable and adds unwanted numbers into our km/h match sometimes so lets try fix it here
                    String[] delimiter = { "km/h" };
                    String[] split1 = s.Split(delimiter, count, StringSplitOptions.RemoveEmptyEntries);
                    String[] delimiter2 = { " " };
                    if(split1.Length > 0)
                    {
                        String[] split2 = split1[0].Split(delimiter2, count, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            // Convert speed string to int
                            int speed = Int32.Parse(Regex.Replace(split2[split2.Length - 1], "[^.0-9]", ""));
                            if (speed > 152) return 0; // faulty result - abort!
                            return speed;
                        }
                        catch (FormatException e)
                        {
                            return 0;
                        }
                    }
                }
            }

            return 0;
        }

        private Dictionary<string, string[]> getPlayerKeybindings()
        {
           ConfigParser ConfigParser = new ConfigParser();
           return ConfigParser.parseConfig();
        }

    }
}
