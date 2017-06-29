﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xabe.FFMpeg;
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
        private string _fileName;
        private int _percent;


        public VideoConverter(IFFMpeg iffmpeg, ISettings settings, ILogger<VideoConverter> logger, IFileProvider provider)
        {
            _iffmpeg = iffmpeg;
            _settings = settings;
            _logger = logger;
            _provider = provider;

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
                FileInfo file = await GetFileToConvert();
                if(file == null)
                    return null;

                using(FileLock.FileLock fileLock = FileLock.FileLock.Acquire(file, TimeSpan.FromSeconds(15), true))
                {
                    if(fileLock == null)
                    {
                        _logger.LogWarning($"File {file.Name} is locked. Skipping.");
                        return null;
                    }

                    outputPath = GetOutputPath(file);

                    if(_settings.saveSourceInfo)
                        SaveSourceInfo(file, outputPath);

                    if(File.Exists(outputPath))
                        File.Delete(outputPath);
                    _fileName = file.Name;

                    _logger.LogInformation($"Start conversion of {_fileName}");
                    await Convert(outputPath, file);
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

        private async Task Convert(string outputPath, FileInfo file)
        {
            if(!await _iffmpeg.ConvertMedia(file, outputPath))
            {
                File.Delete(outputPath);
                _iffmpeg.Dispose();
            }
            else if(_settings.deleteSource)
            {
                await _provider.Delete();
            }
        }

        private async Task<FileInfo> GetFileToConvert()
        {
            FileInfo file = await _provider.GetNext();
            if(file == null)
            {
                await _provider.Refresh();
                file = await _provider.GetNext();
                if(file == null)
                {
                    _logger.LogInformation($"Did not find any files to convert. Waiting.");
                    return null;
                }
            }

            return file;
        }

        private void SaveSourceInfo(FileInfo file, string outputPath)
        {
            var videoInfo = new VideoInfo(file);
            File.WriteAllText(Path.Combine(new FileInfo(outputPath).Directory.FullName, outputPath.ChangeExtension(".info")), videoInfo.ToString());
        }

        private string GetOutputPath(FileInfo file)
        {
            var outputDir = "";
            if(_settings.usePaths)
                if(file.IsTvShow())
                    outputDir = Path.Combine(_settings.serialsPath, $"{file.RemoveTvShowInfo()}", $"Season {file.GetSeason()}");
                else
                    outputDir = Path.Combine(_settings.moviesPath, $"{file.GetNormalizedName()}");
            else
                outputDir = file.Directory.FullName;

            string path = Path.Combine(outputDir, file.GetNormalizedName()
                                                      .ChangeExtension());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    }
}
