﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public class Updater
    {
        private readonly IUpdate _updater;

        public Updater(IUpdate updater)
        {
            _updater = updater;
        }

        public async Task<bool> CheckForUpdate()
        {
            var currentVersion = new Version(await _updater.GetCurrentVersion());
            var installedVersion = new Version(await _updater.GetInstalledVersion());
            return currentVersion > installedVersion;
        }

        public void Update()
        {
            var files = _updater.DownloadCurrentVersion()
                                .Result;
            var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()
                                                        .Location);

            var fileNames = files.Select(Path.GetFileName);

            foreach(var fileName in fileNames)
            {
                var path = Path.Combine(outputDir, fileName);
                if(File.Exists(path))
                {
                    try
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                            .ToString());
                        File.Move(path, tempPath);
                    }
                    catch(FileNotFoundException)
                    {
                    }
                }
            }

            foreach(var file in files)
            {
                var outputPath = Path.Combine(outputDir, Path.GetFileName(file));
                File.Copy(file, outputPath);
            }

            _updater.RestartApp();
        }
    }
}
