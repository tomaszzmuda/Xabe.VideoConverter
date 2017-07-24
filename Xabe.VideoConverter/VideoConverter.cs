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
            FileInfo file = null;

            try
            {
                ILock fileLock;
                do
                {
                    if(file != null)
                    {
                        _logger.LogWarning($"Cannot create lock for file {file.Name}");
                    }
                    file = await _provider.GetNext();
                    if(file == null)
                    {
                        await _provider.Refresh();
                        return null;
                    }
                    fileLock = new FileLock.FileLock(file);
                } while(!await fileLock.TryAcquire(TimeSpan.FromMinutes(15), true));


                using(fileLock)
                {
                    _fileName = file.Name;
                    outputPath = GetOutputPath(file);

                    if(File.Exists(outputPath))
                        File.Delete(outputPath);

                    SaveSourceInfo(file, outputPath);

                    if(file.Extension == ".mp4")
                    {
                        File.Move(file.FullName, outputPath);
                        return outputPath;
                    }
                    else
                    {
                        using(_iffmpeg)
                        {
                            await _iffmpeg.ConvertMedia(file, outputPath);
                        }
                    }

                    Task saveHash = SaveHash(file, outputPath);
                    Task downloadSubtitles = DownloadSubtitles(outputPath, file);
                    Task downloadTrailer = DownloadTrailer(outputPath, file);

                    await Task.WhenAll(saveHash, downloadSubtitles, downloadTrailer);
                    if(_settings.DeleteSource)
                    {
                        file.Delete();
                        _logger.LogInformation($"Deleted file {file.Name}");
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                if(!string.IsNullOrWhiteSpace(outputPath) &&
                   file != null &&
                   file.Exists)
                    File.Delete(outputPath);
                _iffmpeg.Dispose();
                return null;
            }
            return outputPath;
        }

        private async Task DownloadTrailer(string outputPath, FileInfo file)
        {
            if(_settings.DownloadTrailers &&
               !file.IsTvShow())
            {
                await _trailerDownloader.DownloadTrailer(outputPath);
            }
        }

        private async Task DownloadSubtitles(string outputPath, FileInfo file)
        {
            if(_settings.DownloadSubtitles)
            {
                await SubtitleDownloader.SaveSubtitles(file, outputPath);
            }
        }

        private async Task SaveHash(FileInfo file, string outputPath)
        {
            if(!_settings.CreateHash)
            {
                return;
            }
            var hash = HashHelper.GetHash(file);
            await File.WriteAllTextAsync(Path.ChangeExtension(outputPath, ".hash"), hash, Encoding.UTF8);
        }

        private void SaveSourceInfo(FileInfo file, string outputPath)
        {
            if(!_settings.SaveSourceInfo)
                return;

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
