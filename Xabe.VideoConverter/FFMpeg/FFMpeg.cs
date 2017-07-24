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

        public FFMpeg(ILogger<FFMpeg> logger, ISettings settings)
        {
            if(!string.IsNullOrWhiteSpace(settings.ffmpegPath))
            {
                FFBase.FFMpegDir = settings.ffmpegPath;
            }

            _logger = logger;
            _ffmpeg = new Xabe.FFMpeg.FFMpeg();
            _ffmpeg.OnProgress += (duration, totalLength) => OnChange(this, new ConvertProgressEventArgs(duration, totalLength));
        }

        public void Dispose()
        {
            _ffmpeg.Stop();
        }

        public event ChangedEventHandler OnChange = (sender, args) => { };

        public string GetVideoInfo(FileInfo file)
        {
            return new VideoInfo(file).ToString();
        }

        public Task ConvertMedia(FileInfo input, string outputPath)
        {
            return Task.Run(() =>
            {
                var output = new FileInfo(outputPath);
                _ffmpeg.ToMp4(new VideoInfo(input.FullName), output, Speed.Medium, multithread: true);
            });
        }
    }
}
