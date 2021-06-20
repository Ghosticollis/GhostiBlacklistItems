using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GhostiBlacklistItems
{
    public class MConfig
    {
        public static void loadConfigData() {
            try {
                string configFile = @".\Modules\GhostiBlacklistItems\config.txt";
                if (System.IO.File.Exists(configFile)) {
                    string[] configData = System.IO.File.ReadAllLines(configFile);
                    foreach (string str in configData) {
                        if (str.Length > 0 && str[0] != '#') {
                            Match match = Regex.Match(str.Trim(), @"^([\.\w\d\s]+):(.+)", RegexOptions.IgnoreCase);
                            if (match.Success) {
                                string option = match.Groups[1].Value.Trim();
                                string data = match.Groups[2].Value.Trim();
                                if (option.Length > 0 && data.Length > 0) {
                                    parseConfigData(option, data);
                                }
                            }
                        }
                    }
                } else {
                    string[] str = { "# add data here in the following way:- items blacklisted from spawning: 10, 17, 122", "# lines start with the sign # gonna be ignored", "items blacklisted from spawning: 0" };
                    System.IO.File.WriteAllLines(configFile, str);
                    CommandWindow.LogWarning("GhostiBlacklistItems Module: Can't find config file! auto-created empty config file.");
                }
            } catch (Exception e) {
                CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception3 got cought: " + e.Message);
            }
        }

        static void parseConfigData(string option, string data) {
            if (option == "items blacklisted from spawning") {
                var item_ids = data.Split(',');
                foreach (var idStr in item_ids) {
                    ushort id;
                    if (ushort.TryParse(idStr, out id) && id != 0) {
                        Main.spawn_blacklisted_items.Add(id);
                    }
                }
            } else if (option == "enable filtering") {
                Main.bEnableFiltering = (data == "true");
            } else if (option == "vehicles blacklisted from spawning") {
                var item_ids = data.Split(',');
                foreach (var idStr in item_ids) {
                    ushort id;
                    if (ushort.TryParse(idStr, out id) && id != 0) {
                        Main.spawn_blacklisted_vehicles.Add(id);
                    }
                }
            }
        }
    }
}
