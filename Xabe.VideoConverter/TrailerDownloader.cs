using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Logging;
using YoutubeExtractor;

namespace Xabe.VideoConverter
{
    public class TrailerDownloader
    {
        private readonly ILogger<TrailerDownloader> _logger;
        private readonly ISettings _settings;

        public TrailerDownloader(ISettings settings, ILogger<TrailerDownloader> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        private async Task<string> SearchVideo(string name)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = _settings.YoutubeApiKey,
                ApplicationName = GetType()
                    .ToString()
            });
            SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = name;
            searchListRequest.MaxResults = 1;
            searchListRequest.Type = "video";

            SearchListResponse searchListResponse = await searchListRequest.ExecuteAsync();
            SearchResult trailer = searchListResponse.Items.FirstOrDefault();
            if(trailer == null)
                return null;
            return $"https://www.youtube.com/watch?v={trailer.Id.VideoId}";
        }

        public async Task DownloadTrailer(string moviePath)
        {
            string movieName = Path.GetFileNameWithoutExtension(Path.GetFileName(moviePath));
            string link = await SearchVideo($"{movieName} trailer");
            if(link == null)
            {
                _logger.LogWarning($"No trailer for {movieName}");
                return;
            }

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
            VideoInfo video = videoInfos.OrderByDescending(x => x.Resolution)
                                        .First(x => x.VideoType == VideoType.Mp4 && x.AdaptiveType == AdaptiveType.None);

            if(video.RequiresDecryption)
                DownloadUrlResolver.DecryptDownloadUrl(video);

            var videoDownloader = new VideoDownloader(video,
                Path.Combine(Path.GetDirectoryName(moviePath), $"{video.Title.RemoveIllegalCharacters()}-trailer{video.VideoExtension}"));

            _logger.LogInformation($"Start downloading trailer for {movieName}");
            videoDownloader.Execute();
            _logger.LogInformation($"Downloading trailer for {movieName} finished.");
        }
    }
}
