using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xabe.AutoUpdater;

namespace Xabe.VideoConverter
{
    class Updater
    {
        private readonly ILogger<Updater> _logger;
        private readonly IUpdater _updater;
        private JObject _oldSettings;

        public Updater(ILogger<Updater> logger)
        {
            _logger = logger;
            _updater = new AutoUpdater.Updater(new AssemblyVersionChecker(), new GithubProvider("Xabe.VideoConverter", "tomaszzmuda", "Xabe.VideoConverter"));
            _updater.Updating += Updater_Updating;
            _updater.Restarting += Updater_Restarting;
            _updater.CheckedLatestVersionNumber += _updater_CheckedLatestVersionNumber;
            _updater.CheckedInstalledVersionNumber += _updater_CheckedInstalledVersionNumber;
        }

        private void _updater_CheckedInstalledVersionNumber(object sender, Version version)
        {
            _logger.LogInformation($"Installed version number {version}");
        }

        private void _updater_CheckedLatestVersionNumber(object sender, Version version)
        {
            _logger.LogInformation($"Latest version number {version}");
        }

        private void Updater_Restarting(object sender, EventArgs e)
        {
            UpdateSettings();
            _logger.LogInformation($"Restarting application");
        }

        private void Updater_Updating(object sender, EventArgs e)
        {
            _logger.LogInformation($"Start updating");
            _oldSettings = JObject.Parse(File.ReadAllText("settings.json"));
        }

        private void UpdateSettings()
        {
            _logger.LogInformation($"Update settings.json");
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
