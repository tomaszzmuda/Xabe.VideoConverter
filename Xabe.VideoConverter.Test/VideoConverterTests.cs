using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using Xabe.VideoConverter.FFMpeg;
using Xabe.VideoConverter.Providers;
using Xunit;

namespace Xabe.VideoConverter.Test
{
    public class VideoConverterTests
    {
        public VideoConverterTests()
        {
            _tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                                      .ToString()))
                                .FullName;

            _settings = new Mock<ISettings>();
            _ffmpeg = new Mock<IFFMpeg>();
            _logger = new Mock<ILogger<VideoConverter>>();
            _provider = new Mock<IFileProvider>();

            _tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                                      .ToString()))
                                .FullName;

            _ffmpeg.Raise(x => x.OnChange += null, EventArgs.Empty);
            _ffmpeg.Setup(x => x.ConvertMedia(It.IsAny<FileInfo>(), It.IsAny<string>()))
                   .Returns(() => null);
            _ffmpeg.Setup(x => x.Dispose())
                   .Verifiable();

            _logger.Setup(x => x.Log(LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString()
                                                .Contains("CreateInvoiceFailed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()))
                   .Verifiable();

            _settings.Setup(x => x.DeleteSource)
                     .Returns(false);
            _settings.Setup(x => x.Inputs)
                     .Returns(new[] {_tempDir});
            _settings.Setup(x => x.UsePaths)
                     .Returns(false);
            _settings.Setup(x => x.SaveSourceInfo)
                     .Returns(false);
        }

        private readonly Mock<ISettings> _settings;
        private readonly string _tempDir;
        private readonly Mock<ILogger<VideoConverter>> _logger;
        private readonly Mock<IFFMpeg> _ffmpeg;
        private readonly Mock<IFileProvider> _provider;

        [Fact]
        public async void Execute()
        {
            _provider.Setup(x => x.GetNext())
                     .ReturnsAsync(new FileInfo(Path.GetTempFileName()));

            _ffmpeg.Setup(x => x.ConvertMedia(It.IsAny<FileInfo>(), It.IsAny<string>()))
                   .ReturnsAsync(() => true);

            var videoConverter = new VideoConverter(_ffmpeg.Object, _settings.Object, _logger.Object, _provider.Object);
            string result = await videoConverter.Execute();
            Assert.NotNull(result);
        }

        [Fact]
        public async void ExecuteEmptyQueue()
        {
            _provider.Setup(x => x.GetNext())
                     .ReturnsAsync(() => null);

            var videoConverter = new VideoConverter(_ffmpeg.Object, _settings.Object, _logger.Object, _provider.Object);
            string result = await videoConverter.Execute();
            Assert.Null(result);
        }
    }
}
