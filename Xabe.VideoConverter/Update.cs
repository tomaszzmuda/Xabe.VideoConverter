﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Octokit;
using Xabe.AutoUpdater;

namespace Xabe.VideoConverter
{
    internal class Update: IUpdate
    {
        private readonly ILogger<Update> _logger;
        private JObject _oldSettings;

        public Update(ILogger<Update> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> GetCurrentVersion()
        {
            _logger.LogInformation("Getting current version number.");
            var client = new GitHubClient(new ProductHeaderValue("Xabe.VideoConverter"));
            var releases = await client.Repository.Release.GetAll("tomaszzmuda", "Xabe.VideoConverter");
            var latest = releases[0];
            _logger.LogInformation($"Current version number: {latest.TagName}.");
            return latest.TagName;
        }

        /// <inheritdoc />
        public async Task<string> GetInstalledVersion()
        {
            Version version = Assembly.GetEntryAssembly()
                                      .GetName()
                                      .Version;
            string versionNumber = string.Join('.', version.Major, version.Minor, version.Build);
            _logger.LogInformation($"Installed version number: {versionNumber}.");
            return versionNumber;
        }

        /// <inheritdoc />
        public void RestartApp()
        {
            UpdateSettings();
            _logger.LogInformation("Restarting app.");
            var args = Environment.GetCommandLineArgs();
            System.Diagnostics.Process.Start("dotnet ", string.Join(' ', args));
            Environment.Exit(0);
        }

        private void UpdateSettings()
        {
            JObject newSettings = JObject.Parse(File.ReadAllText("settings.json"));
            foreach(var setting in newSettings)
            {
                try
                {
                    var value = _oldSettings[setting.Key].Value<dynamic>();
                    newSettings[setting.Key] = value;
                }
                catch(Exception) { }
            }
            File.WriteAllText("settings.json", newSettings.ToString());
        }

        /// <inheritdoc />
        public async Task<List<string>> DownloadCurrentVersion()
        {
            _oldSettings = JObject.Parse(File.ReadAllText("settings.json"));
            _logger.LogInformation("Downloading latest version.");
            var client = new GitHubClient(new ProductHeaderValue("Xabe.VideoConverter"));
            var releases = await client.Repository.Release.GetAll("tomaszzmuda", "Xabe.VideoConverter");
            var latest = releases[0];
            var url = latest.Assets[0].BrowserDownloadUrl;
            var tempFile = Path.GetTempFileName();
            using(var webClient = new WebClient())
            {
                webClient.DownloadFile(url, tempFile);
            }
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                 .ToString());
            System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, outputDir);

            var files = Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories);

            return files.Where(x => !x.Contains("settings.json"))
                        .ToList();
        }
    }
}
