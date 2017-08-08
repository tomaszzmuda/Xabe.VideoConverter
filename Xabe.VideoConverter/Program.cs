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
                .AddTransient<IFileProvider, RecursiveProvider>()
                .AddTransient<TrailerDownloader>()
                .AddTransient<Update>()
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = services.GetService<ILoggerFactory>();
            Logger.Init(loggerFactory, _settings);

            var updater = new Updater(services.GetService<Update>());

            while(true)
            {
                if(updater.CheckForUpdate()
                          .Result)
                {
                    updater.Update();
                }
                var conversionResult = false;
                try
                {
                    Task.Run(async () =>
                    {
                        var videoConverter = services.GetService<VideoConverter>();
                        conversionResult = await videoConverter.Execute();
                    })
                        .Wait();
                }
                catch(Exception)
                {
                }
                if(!conversionResult)
                    Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
    }
}
