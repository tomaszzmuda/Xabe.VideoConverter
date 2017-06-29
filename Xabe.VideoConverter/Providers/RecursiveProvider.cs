using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Xabe.VideoConverter.Providers
{
    public class RecursiveProvider: IFileProvider
    {
        private readonly ILogger<RecursiveProvider> _logger;
        private readonly ISettings _settings;
        private FileInfo _currentFile;
        private DirectoryInfo _directory;
        private Queue<FileInfo> _fileList;

        public RecursiveProvider(ISettings settings, ILogger<RecursiveProvider> logger)
        {
            _settings = settings;
            _logger = logger;
            _fileList = new Queue<FileInfo>();

            foreach(var inputDir in _settings.Inputs)
            {
                if(!new DirectoryInfo(inputDir).Exists)
                    throw new IOException($"Directory {inputDir} doesn't exist.");
            }

            Task.Run(async () => await Refresh())
                .Wait();
        }

        public async Task<FileInfo> GetNext()
        {
            return await Task.Run(() =>
            {
                _logger.LogInformation($"Get next item from queue");
                _fileList.TryDequeue(out _currentFile);
                return _currentFile;
            });
        }

        public async Task Delete()
        {
            await Task.Run(() =>
            {
                _currentFile.Delete();
                _logger.LogInformation($"Deleted file {_currentFile.Name}");
            });
        }

        public async Task Refresh()
        {
            _fileList = new Queue<FileInfo>();
            await UpdateFileList();
        }

        private async Task UpdateFileList()
        {
            await Task.Run(() =>
            {
                foreach(string inputDirectory in _settings.Inputs)
                {
                    _directory = new DirectoryInfo(inputDirectory);
                    List<FileInfo> allFiles = _directory.GetFiles("*", SearchOption.AllDirectories)
                                                        .ToList();
                    allFiles = allFiles.ToList()
                                       .FindAll(x => _settings.Extensions.Contains(x.Extension) && x.Length >= _settings.MinFileSize * 8 * 1024)
                                       .ToList();
                    foreach(FileInfo file in allFiles.OrderBy(x => x.Name))
                        _fileList.Enqueue(file);
                }

                _logger.LogInformation($"Update file list. Found {_fileList.Count} files");
            });
        }
    }
}
