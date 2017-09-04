using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xabe.FFMpeg;

namespace Xabe.VideoConverter.FFMpeg
{
    [UsedImplicitly]
    public class FFMpeg: IFFMpeg
    {
        private IConversion _conversion;
        private readonly object _conversionLock = new object();

        private IConversion Conversion
        {
            get
            {
                lock(_conversionLock)
                {
                    if(_conversion == null)
                    {
                        _conversion = ConversionHelper.ToMp4("", "", multithread: true);   
                        _conversion.OnProgress += (duration, totalLength) => OnChange(this, new ConvertProgressEventArgs(duration, totalLength));
                    }
                    return _conversion;
                }
            }
        }

        public FFMpeg(ISettings settings)
        {
            if(!string.IsNullOrWhiteSpace(settings.FFMpegPath))
                FFBase.FFMpegDir = settings.FFMpegPath;
        }

        public void Dispose()
        {
            _conversion?.Dispose();
        }

        public event ChangedEventHandler OnChange = (sender, args) => { };

        public string GetVideoInfo(FileInfo file)
        {
            return new VideoInfo(file).ToString();
        }

        public async Task ConvertMedia(FileInfo input, string outputPath)
        {
            await Conversion.SetInput(input)
                       .SetOutput(outputPath)
                       .Start();
        }
    }
}
