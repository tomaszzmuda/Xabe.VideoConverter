using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xabe.AutoUpdater;

namespace Xabe.VideoConverter
{
    internal class Update: IUpdate
    {
        public string GetCurrentVersion()
        {
            return "1.0.1";
        }

        public string GetInstalledVersion()
        {
            return "1.0.1";
        }

        public List<string> DownloadCurrentVersion()
        {
            var files = Directory.GetFiles("C:\\Users\\tzmuda.POSTDATA\\Desktop\\VideoConverter", "*", SearchOption.AllDirectories);
            return files.ToList();
        }

        public void RestartApp()
        {
            var args = Environment.GetCommandLineArgs();
            System.Diagnostics.Process.Start("dotnet ", string.Join(' ', args));
            Environment.Exit(0);
        }
    }
}
