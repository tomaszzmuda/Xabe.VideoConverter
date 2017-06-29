using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Xabe.VideoConverter
{
    internal class Logger
    {
        private Logger()
        {
        }

        public static void Init(ILoggerFactory factory, ISettings settings)
        {
            factory.AddConsole();
            factory.AddFile(Path.Combine(settings.log.errorPath, $"error_{Environment.MachineName}.log"), LogLevel.Error);
            factory.AddFile(Path.Combine(settings.log.infoPath, $"info_{Environment.MachineName}.log"));
        }
    }
}
