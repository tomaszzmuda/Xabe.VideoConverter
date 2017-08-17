using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xabe.FFMpeg;
using Xabe.FFMpeg.Enums;

namespace Xabe.VideoConverter.FFMpeg
{
    public class FFMpeg: IFFMpeg
    {
        private readonly ILogger<FFMpeg> _logger;
        private IVideoInfo _currentVideo;

        public FFMpeg(ILogger<FFMpeg> logger, ISettings settings)
        {
            if(!string.IsNullOrWhiteSpace(settings.ffmpegPath))
                FFBase.FFMpegDir = settings.ffmpegPath;

            _logger = logger;
        }

        public void Dispose()
        {
            _currentVideo?.Dispose();
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
                _currentVideo = new VideoInfo(input);
                _currentVideo.OnConversionProgress += (duration, totalLength) => OnChange(this, new ConvertProgressEventArgs(duration, totalLength));
                _currentVideo.ToMp4(outputPath, Speed.Medium, multithread: true);
            });
        }
    }
}
