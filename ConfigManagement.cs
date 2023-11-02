using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace ChatUtilities
{
    public static class ConfigManagement
    {
        public static ConfigFile config;        
        public static ConfigEntry<KeyCode> emoticonKey;
        public static ConfigEntry<KeyCode> commandKey;
        public static ConfigEntry<KeyCode> clipboardKey;
        public static ConfigEntry<KeyCode> clearKey;
        public static ConfigEntry<KeyCode> closeKey;
        public static ConfigEntry<string> autoSendSuffix;

        public static List<ConfigEntry<string>> commandConfigEntries = new List<ConfigEntry<string>>();
        private static int commandCount = 9;

        public static void Initialize(ConfigFile cfg)
        {
            config = cfg;

            emoticonKey = config.Bind("Controls", "Emoticon Key", KeyCode.LeftControl, "");
            commandKey = config.Bind("Controls", "Command Key", KeyCode.LeftAlt, "");
            clipboardKey = config.Bind("Controls", "Clipboard Key", KeyCode.PageDown, "");
            clearKey = config.Bind("Controls", "Clear Key", KeyCode.Delete, "");
            closeKey = config.Bind("Controls", "Close Key", KeyCode.End, "");
            autoSendSuffix = config.Bind("Preferences", "Auto Send Suffix", "<send>", "");

            for(int i = 0; i < commandCount; i++)
            {
                commandConfigEntries.Add(config.Bind("Commands", "Command " + (i + 1), "", ""));
            }
        }

        //Returns all commands that have any value assigned through the settings.
        public static List<string> GetCommands()
        {
            List<string> commands = new List<string>();

            for (int i = 0; i < commandCount; i++)
            {
                string key = "Command " + (i + 1);
                try
                {
                    string configValue = (string)config["Commands", key].BoxedValue;
                    if (!string.IsNullOrEmpty(configValue))
                    {
                        commands.Add(configValue);
                    }
                }
                catch { }
            }

            return commands;
        }
    }
}
