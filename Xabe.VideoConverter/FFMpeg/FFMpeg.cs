using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xabe.FFMpeg;
using Xabe.FFMpeg.Enums;

namespace Xabe.VideoConverter.FFMpeg
{
    public class FFMpeg: IFFMpeg
    {
        private readonly Xabe.FFMpeg.FFMpeg _ffmpeg;
        private readonly ILogger<FFMpeg> _logger;

        public FFMpeg(ILogger<FFMpeg> logger)
        {
            _logger = logger;
            _ffmpeg = new Xabe.FFMpeg.FFMpeg();
            _ffmpeg.OnProgress += (duration, totalLength) => OnChange(this, new ConvertProgressEventArgs(duration, totalLength));
        }

        public void Dispose()
        {
            _ffmpeg.Stop();
        }

        public event ChangedEventHandler OnChange = (sender, args) => { };

        public Task<bool> ConvertMedia(FileInfo input, string outputPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    var output = new FileInfo(outputPath);
                    return _ffmpeg.ToMp4(new VideoInfo(input.FullName), output, Speed.Medium, multithread: true);
                }
                catch(Exception e)
                {
                    _ffmpeg.Stop();
                    _logger.LogError(e.ToString());
                    return false;
                }
            });
        }

        public string GetVideoInfo(FileInfo file)
        {
            return new VideoInfo(file).ToString();
        }
    }
}
