using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using Xabe.VideoConverter.Providers;
using Xunit;

namespace Xabe.VideoConverter.Test
{
    public class FileProviderTests
    {
        public FileProviderTests()
        {
            _settings = new Mock<ISettings>();
            _logger = new Mock<ILogger<RecursiveProvider>>();
            _tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                                      .ToString()))
                                .FullName;
            _settings.Setup(x => x.Inputs)
                     .Returns(new[] {_tempDir});
            _settings.Setup(x => x.Extensions)
                     .Returns(new[] {Extension});

            _logger.Setup(x => x.Log(LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString()
                                                .Contains("CreateInvoiceFailed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()))
                   .Verifiable();
        }

        private readonly Mock<ISettings> _settings;
        private readonly string _tempDir;
        private readonly Mock<ILogger<RecursiveProvider>> _logger;
        private const string Extension = ".rprodiver";

        [Fact]
        public async void GetNext()
        {
            string tmpFilePath = Path.Combine(_tempDir, Path.ChangeExtension(Path.GetRandomFileName(), Extension));
            File.Create(tmpFilePath);
            var recursiveProvider = new RecursiveProvider(_settings.Object, _logger.Object);

            FileInfo nextFile = await recursiveProvider.GetNext();

            Assert.Equal(tmpFilePath, nextFile.FullName);
        }

        [Fact]
        public async void GetNextEmpty()
        {
            var recursiveProvider = new RecursiveProvider(_settings.Object, _logger.Object);

            FileInfo nextFile = await recursiveProvider.GetNext();

            Assert.Null(nextFile);
        }

        [Fact]
        public async void GetNextToEmpty()
        {
            string tmpFilePath = Path.Combine(_tempDir, Path.ChangeExtension(Path.GetRandomFileName(), Extension));
            File.Create(tmpFilePath);
            var recursiveProvider = new RecursiveProvider(_settings.Object, _logger.Object);

            FileInfo nextFile = await recursiveProvider.GetNext();
            nextFile = await recursiveProvider.GetNext();

            Assert.Null(nextFile);
        }

        [Fact]
        public async void GetNextWithDelete()
        {
            string tmpFilePathA = Path.Combine(_tempDir, Path.ChangeExtension("a" + Path.GetRandomFileName(), Extension));
            File.Create(tmpFilePathA)
                .Close();
            string tmpFilePathB = Path.Combine(_tempDir, Path.ChangeExtension("b" + Path.GetRandomFileName(), Extension));
            File.Create(tmpFilePathB)
                .Close();

            var recursiveProvider = new RecursiveProvider(_settings.Object, _logger.Object);
            File.Delete(tmpFilePathA);

            FileInfo nextFile = await recursiveProvider.GetNext();

            Assert.NotNull(nextFile);
        }

        [Fact]
        public async void Refresh()
        {
            string tmpFilePath = Path.Combine(_tempDir, Path.ChangeExtension(Path.GetRandomFileName(), Extension));
            File.Create(tmpFilePath)
                .Close();

            var recursiveProvider = new RecursiveProvider(_settings.Object, _logger.Object);

            FileInfo nextFile = await recursiveProvider.GetNext();

            await recursiveProvider.Refresh();

            nextFile = await recursiveProvider.GetNext();

            Assert.NotNull(nextFile);
        }
    }
}
