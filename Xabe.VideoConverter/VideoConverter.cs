using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xabe.FileLock;
using Xabe.VideoConverter.FFMpeg;
using Xabe.VideoConverter.Providers;

namespace Xabe.VideoConverter
{
    public class VideoConverter
    {
        private readonly IFFMpeg _iffmpeg;
        private readonly ILogger<VideoConverter> _logger;
        private readonly IFileProvider _provider;
        private readonly ISettings _settings;
        private readonly TrailerDownloader _trailerDownloader;
        private string _fileName;
        private int _percent;


        public VideoConverter(IFFMpeg iffmpeg, ISettings settings, ILogger<VideoConverter> logger, IFileProvider provider, TrailerDownloader trailerDownloader)
        {
            _iffmpeg = iffmpeg;
            _settings = settings;
            _logger = logger;
            _provider = provider;
            _trailerDownloader = trailerDownloader;

            _iffmpeg.OnChange += (sender, e) => _ffmpeg_ConvertProgress(e, _fileName);
        }

        private void _ffmpeg_ConvertProgress(ConvertProgressEventArgs e, string fileName)
        {
            if(e.Percent == _percent)
                return;
            _logger.LogInformation($"{fileName} [{e.Duration} / {e.TotalLength}] {e.Percent}%");
            _percent = e.Percent;
        }

        public async Task<string> Execute()
        {
            var outputPath = "";
            try
            {
                FileInfo file;
                ILock fileLock;
                do
                {
                    file = await _provider.GetNext();
                    if(file == null)
                    {
                        return null;
                    }
                    fileLock = new FileLock.FileLock(file);
                    if(fileLock == null)
                    {
                        break;
                    }
                } while(!await fileLock.TryAcquire(TimeSpan.FromMinutes(15), true));


                using(fileLock)
                {
                    _fileName = file.Name;
                    outputPath = GetOutputPath(file);

                    if(File.Exists(outputPath))
                        File.Delete(outputPath);

                    if(_settings.SaveSourceInfo)
                        SaveSourceInfo(file, outputPath);

                    if(_settings.CreateHash)
                    {
                        SaveHash(file, outputPath);
                    }
                    if(_settings.DownloadSubtitles)
                    {
                        await SubtitleDownloader.SaveSubtitles(file, outputPath);
                    }

                    _logger.LogInformation($"Start conversion of {_fileName}");
                    await Convert(outputPath, file);

                    if(_settings.DownloadTrailers)
                    {
                        await _trailerDownloader.DownloadTrailer(outputPath);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                if(!string.IsNullOrWhiteSpace(outputPath))
                    File.Delete(outputPath);
                return null;
            }
            return outputPath;
        }

        private static void SaveHash(FileInfo file, string outputPath)
        {
            var hash = HashHelper.GetHash(file);
            File.WriteAllText(Path.ChangeExtension(outputPath, ".hash"), hash, Encoding.UTF8);
        }

        private async Task Convert(string outputPath, FileInfo file)
        {
            if(!await _iffmpeg.ConvertMedia(file, outputPath))
            {
                File.Delete(outputPath);
                _iffmpeg.Dispose();
            }
            else if(_settings.DeleteSource)
            {
                await _provider.Delete();
            }
        }

        private void SaveSourceInfo(FileInfo file, string outputPath)
        {
            File.WriteAllText(Path.Combine(new FileInfo(outputPath).Directory.FullName, outputPath.ChangeExtension(".info")), _iffmpeg.GetVideoInfo(file));
        }

        private string GetOutputPath(FileInfo file)
        {
            var outputDir = "";
            if(_settings.UsePaths)
                if(file.IsTvShow())
                    outputDir = Path.Combine(_settings.SerialsPath, $"{file.GetNormalizedName() .RemoveTvShowInfo()}", $"Season {file.GetNormalizedName() .GetSeason()}");
                else
                    outputDir = Path.Combine(_settings.MoviesPath, $"{file.GetNormalizedName()}");
            else
                outputDir = file.Directory.FullName;

            string path = Path.Combine(outputDir, file.GetNormalizedName()
                                                      .ChangeExtension());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    }
}
