using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Xabe.VideoConverter
{
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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .Build();
        }
    }

    public interface ISettings
    {
        string[] extensions { get; set; }
        Log log { get; set; }
        string[] inputs { get; set; }
        int minFileSize { get; set; }
        bool deleteSource { get; set; }
        bool usePaths { get; set; }
        string moviesPath { get; set; }
        string serialsPath { get; set; }
        string youtubeApiKey { get; set; }
        bool downloadTrailers { get; set; }
        bool saveSourceInfo { get; set; }
    }

    public class Settings: ISettings
    {
        public string[] extensions { get; set; }
        public Log log { get; set; }
        public string[] inputs { get; set; }
        public int minFileSize { get; set; }
        public bool deleteSource { get; set; }
        public bool usePaths { get; set; }
        public string moviesPath { get; set; }
        public string serialsPath { get; set; }
        public string youtubeApiKey { get; set; }
        public bool downloadTrailers { get; set; }
        public bool saveSourceInfo { get; set; }
    }

    public class Log
    {
        public string errorPath { get; set; }
        public string infoPath { get; set; }
    }
}
