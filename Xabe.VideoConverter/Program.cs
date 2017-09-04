using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xabe.VideoConverter.FFMpeg;
using Xabe.VideoConverter.Providers;

namespace Xabe.VideoConverter
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            ServiceProvider services = InitalizeServices();

            var settings = services.GetService<ISettings>();
            var loggerFactory = services.GetService<ILoggerFactory>();
            Logger.Init(loggerFactory, settings);

            var updater = services.GetService<Updater>();

            while (true)
            {
                CheckForUpdate(updater, settings);
                ConvertVideo(services);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static ServiceProvider InitalizeServices()
        {
            var settings = Configuration.GetConfiguration()
                                     .Get<Settings>();

            return new ServiceCollection()
                .AddSingleton<Configuration, Configuration>()
                .AddSingleton<ISettings>(settings)
                .AddSingleton<VideoConverter>()
                .AddSingleton<Updater>()
                .AddSingleton<IFFMpeg, FFMpeg.FFMpeg>()
                .AddTransient<IFileProvider, RecursiveProvider>()
                .AddTransient<TrailerDownloader>()
                .AddLogging()
                .BuildServiceProvider();
        }

        private static void ConvertVideo(ServiceProvider services)
        {
            var conversionResult = false;
            try
            {
                var videoConverter = services.GetService<VideoConverter>();
                conversionResult = videoConverter.Execute().Result;
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            if (!conversionResult)
                Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        private static void CheckForUpdate(Updater updater, ISettings settings)
        {
            if (settings.AutoUpdate && updater.IsUpdateAvaiable()
                      .Result)
            {
                updater.Update();
            }
        }
    }
}
