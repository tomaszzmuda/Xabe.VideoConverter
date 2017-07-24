using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = services.GetService<ILoggerFactory>();
            Logger.Init(loggerFactory, _settings);

            while(true)
            {
                bool conversionResult = false;
                Task.Run(async () => { conversionResult = await FormatVideo(services); })
                    .Wait();
                if(!conversionResult)
                    Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        private static async Task<bool> FormatVideo(ServiceProvider services)
        {
            var videoConverter = services.GetService<VideoConverter>();
            return await videoConverter.Execute();
        }
    }
}
