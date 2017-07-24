using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestResponse = RestSharp.RestResponse;

namespace Xabe.VideoConverter
{
    internal class SubtitleDownloader
    {
        public static async Task SaveSubtitles(FileInfo file, string outputPath)
        {
            string hash = HashHelper.GetHash(file);
            var client = new RestClient("http://opensubtitlesapi.azurewebsites.net/api/OpenSubtitles/GetByHash");
            var request = new RestRequest();
            request.AddParameter("hash", hash);
            var result = await client.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                var response = JsonConvert.DeserializeObject<byte[]>(result.Content);
                await File.WriteAllBytesAsync(Path.ChangeExtension(outputPath, ".pol.srt"), response);
            }
        }
    }

    public static partial class Extensions
    {
        public static async Task<RestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            var taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse) await taskCompletion.Task;
        }
    }
}
