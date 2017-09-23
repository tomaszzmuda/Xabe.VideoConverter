using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Xabe.VideoConverter
{
    [UsedImplicitly]
    public class Configuration
    {
        private Configuration()
        {
        }

        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddCommandLine(Environment.GetCommandLineArgs()
                                           .Skip(1)
                                           .ToArray())
                .SetBasePath(Assembly.GetEntryAssembly().Location)
                .AddJsonFile("settings.json", false, true)
                .Build();
        }
    }

    public interface ISettings
    {
        string[] Extensions { get; set; }
        Log Log { get; set; }
        string[] Inputs { get; set; }
        int MinFileSize { get; set; }
        bool DeleteSource { get; set; }
        bool UsePaths { get; set; }
        string MoviesPath { get; set; }
        string SerialsPath { get; set; }
        string YoutubeApiKey { get; set; }
        bool DownloadTrailers { get; set; }
        bool SaveSourceInfo { get; set; }
        bool CreateHash { get; set; }
        bool DownloadSubtitles { get; set; }
        string FFMpegPath { get; set; }
        bool AutoUpdate { get; set; }
    }

    [UsedImplicitly]
    public class Settings: ISettings
    {
        public string[] Extensions { get; set; }
        public Log Log { get; set; }
        public string[] Inputs { get; set; }
        public int MinFileSize { get; set; }
        public bool DeleteSource { get; set; }
        public bool UsePaths { get; set; }
        public string MoviesPath { get; set; }
        public string SerialsPath { get; set; }
        public string YoutubeApiKey { get; set; }
        public bool DownloadTrailers { get; set; }
        public bool SaveSourceInfo { get; set; }
        public bool CreateHash { get; set; }
        public bool DownloadSubtitles { get; set; }
        public string FFMpegPath { get; set; }
        public bool AutoUpdate { get; set; }
    }

    [UsedImplicitly]
    public class Log
    {
        public string ErrorPath { get; set; }
        public string InfoPath { get; set; }
    }
}
