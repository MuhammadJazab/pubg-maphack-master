using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PUBG_MAPHACK
{
    public class ConfigParser
    {
        public string playerConfig = "";
        public Dictionary<string, string> enumsTranslation = new Dictionary<string, string>();
        Dictionary<string, string[]> defaults = new Dictionary<string, string[]>();
        public ConfigParser()
        {
            // Set default bindings as fallback
            defaults.Add("Fire", new string[] { "LButton" });
            defaults.Add("Interact", new string[] { "F" });
            defaults.Add("ResolutionSizeY", new string[] { Screen.PrimaryScreen.Bounds.Height.ToString() });
            defaults.Add("ResolutionSizeX", new string[] { Screen.PrimaryScreen.Bounds.Width.ToString() });
            defaults.Add("StartCookingThrowable", new string[] { "R" });
            
            // Get enum translations
            enumsTranslation = getEnumTranslation();

            // Load pubg player config
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TslGame\Saved\Config\WindowsNoEditor\GameUserSettings.ini"))
            {
                try
                {
                    playerConfig = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TslGame\Saved\Config\WindowsNoEditor\GameUserSettings.ini");
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public Dictionary<string, string[]> parseConfig()
        {
            string keyName;
            string keyBind;
            var keyBindings = new Dictionary<string, string[]>();
            var returnValue = new Dictionary<string, string[]>();
            string[] splitter = { };
            string[] SplitOnKeys = { };
            string[] SplitOnKey = { };
            int i = 0;
            int i2 = 0;

            if (playerConfig.Length > 0)
            {
                /* This is ugly but gets the job done */
                string[] Actions = playerConfig.Split(new[] { "ActionName" }, StringSplitOptions.RemoveEmptyEntries);
                i = 0;
                foreach (var Action in Actions)
                {
                    i++;
                    if (i == 1 || i == Actions.Length) continue; // Skip first and last

                    // Get key name
                    splitter = Action.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitter.Length > 0)
                    {
                        keyName = splitter[1];
                    } else
                    {
                        continue;
                    }

                    // Get key bindings
                    SplitOnKeys = Action.Split(new[] { "Keys=" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var Keys in SplitOnKeys)
                    {
                        if (!Keys.Contains("Key=")) continue;
                        SplitOnKey = Action.Split(new[] { "Key=" }, StringSplitOptions.RemoveEmptyEntries);
                        i2 = 0;
                        foreach (var Key in SplitOnKey)
                        {
                            i2++;
                            if (i2 == 1) continue;
                            // Clean out some not needed characters
                            keyBind = Key;
                            keyBind = keyBind.Replace("),())),(", "");
                            keyBind = keyBind.Replace("))),(", "");
                            keyBind = keyBind.Replace("),(", "");
                            splitter = keyBind.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            if (splitter.Length > 0)
                            {
                                // Translate to enum value
                                if(enumsTranslation.ContainsKey(keyBind)) {
                                    keyBind = enumsTranslation[keyBind];
                                }
                                if(!keyBindings.ContainsKey(keyName))
                                {
                                    // Add new keyBind
                                    keyBindings.Add(keyName, new string[] { keyBind });
                                } else
                                {
                                    // If keyBind has multiple keys lets add second bind also
                                    keyBindings[keyName] = new string[] { keyBind, keyBindings[keyName][0] };
                                }
                                
                            }
                        }
                    }
                }

                // Get player game resolution
                string[] configLines = playerConfig.Split(new[] { '\n' });
                foreach(var line in configLines)
                {
                    if(line.Contains("ResolutionSizeX") && !keyBindings.ContainsKey("ResolutionSizeX"))
                    {
                        keyBindings.Add("ResolutionSizeX", new string[] { line.Replace("ResolutionSizeX=", "") });
                    }
                    if (line.Contains("ResolutionSizeY") && !keyBindings.ContainsKey("ResolutionSizeY"))
                    {
                        keyBindings.Add("ResolutionSizeY", new string[] { line.Replace("ResolutionSizeY=", "") });
                    }
                }

            }

            if (keyBindings.ContainsKey("Fire") && 
                keyBindings.ContainsKey("Interact") && 
                keyBindings.ContainsKey("ResolutionSizeY") && 
                keyBindings.ContainsKey("ResolutionSizeX") && 
                keyBindings.ContainsKey("StartCookingThrowable"))
            {
                returnValue = keyBindings;
            } else
            {
                returnValue = defaults;
            }

            return returnValue;

        }

        public Dictionary<string, string> getEnumTranslation()
        {
            Dictionary<string, string> translation = new Dictionary<string, string>();
            translation.Add("LeftMouseButton", "LButton");
            translation.Add("RightMouseButton", "RButton");
            translation.Add("MiddleMouseButton", "MButton");
            translation.Add("XButton1", "ThumbMouseButton1");
            translation.Add("XButton2", "ThumbMouseButton2");
            translation.Add("LeftShift", "LShiftKey");
            translation.Add("RightShift", "RShiftKey");
            translation.Add("LeftControl", "LControlKey");
            translation.Add("RightControl", "RControlKey");
            translation.Add("SpaceBar", "Space");
            translation.Add("BackSpace", "Back");
            translation.Add("Zero", "D0");
            translation.Add("One", "D1");
            translation.Add("Two", "D2");
            translation.Add("Three", "D3");
            translation.Add("Four", "D4");
            translation.Add("Five", "D5");
            translation.Add("Six", "D6");
            translation.Add("Seven", "D7");
            translation.Add("Eight", "D8");
            translation.Add("Nine", "D9");
            translation.Add("NumPadZero", "NumPad0");
            translation.Add("NumPadOne", "NumPad1");
            translation.Add("NumPadTwo", "NumPad2");
            translation.Add("NumPadThree", "NumPad3");
            translation.Add("NumPadFour", "NumPad4");
            translation.Add("NumPadFive", "NumPad5");
            translation.Add("NumPadSix", "NumPad6");
            translation.Add("NumPadSeven", "NumPad7");
            translation.Add("NumPadEight", "NumPad8");
            translation.Add("NumPadNine", "NumPad9");
            return translation;
        }
    }
}
