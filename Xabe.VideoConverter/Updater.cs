using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xabe.AutoUpdater;

namespace Xabe.VideoConverter
{
    class Updater
    {
        private readonly IUpdater _updater;
        private JObject _oldSettings;

        public Updater()
        {
            _updater = new AutoUpdater.Updater(new AssemblyVersionChecker(), new GithubProvider("Xabe.VideoConverter", "tomaszzmuda", "Xabe.VideoConverter"));
            _updater.Updating += Updater_Updating;
            _updater.Restarting += Updater_Restarting;
        }

        private void Updater_Restarting(object sender, EventArgs e)
        {
            UpdateSettings();
        }

        private void Updater_Updating(object sender, EventArgs e)
        {
            _oldSettings = JObject.Parse(File.ReadAllText("settings.json"));
        }

        private void UpdateSettings()
        {
            JObject newSettings = JObject.Parse(File.ReadAllText(_updater.DownloadedFiles.First(x => x.Contains("settings.json"))));
            foreach (var setting in newSettings)
            {
                try
                {
                    var value = _oldSettings[setting.Key].Value<dynamic>();
                    newSettings[setting.Key] = value;
                }
                catch (Exception)
                {
                }
            }
            File.WriteAllText("settings.json", newSettings.ToString());
        }

        public async Task<bool> IsUpdateAvaiable()
        {
            return await _updater.IsUpdateAvaiable();
        }

        public void Update()
        {
            _updater.Update();
        }
    }
}
