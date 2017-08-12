using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xabe.AutoUpdater;
using Xabe.VideoConverter.FFMpeg;
using Xabe.VideoConverter.Providers;

namespace Xabe.VideoConverter
{
    internal static class Program
    {
        private static Settings _settings;

        public static void Main(string[] args)
        {
            _settings = Configuration.GetConfiguration()
                                     .Get<Settings>();

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Configuration, Configuration>()
                .AddSingleton<ISettings>(_settings)
                .AddSingleton<VideoConverter>()
                .AddSingleton<IFFMpeg, FFMpeg.FFMpeg>()
                .AddSingleton<Updater>()
                .AddSingleton<IUpdate, Update>()
                .AddTransient<IFileProvider, RecursiveProvider>()
                .AddTransient<TrailerDownloader>()
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = services.GetService<ILoggerFactory>();
            Logger.Init(loggerFactory, _settings);

            var updater = services.GetService<Updater>();
            var settings = services.GetService<ISettings>();

            while(true)
            {
                CheckForUpdate(updater, settings);
                ConvertVideo(services);
            }
        }

        private static void ConvertVideo(ServiceProvider services)
        {
            var conversionResult = false;
            try
            {
                var videoConverter = services.GetService<VideoConverter>();
                conversionResult = videoConverter.Execute().Result;
            }
            catch (Exception)
            {
            }
            if (!conversionResult)
                Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        private static void CheckForUpdate(Updater updater, ISettings settings)
        {
            if (settings.autoUpdate && updater.CheckForUpdate()
                      .Result)
            {
                updater.Update();
            }
        }
    }
}
