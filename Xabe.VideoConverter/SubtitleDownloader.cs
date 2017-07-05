using System.Threading.Tasks;
using RestSharp;
using RestResponse = RestSharp.RestResponse;

namespace Xabe.VideoConverter
{
    class SubtitleDownloader
    {
        public static async Task<string> GetSubtitles(string hash)
        {
            var client = new RestClient("http://opensubtitlesapi.azurewebsites.net/api/OpenSubtitles/GetByHash");
            var request = new RestRequest();
            request.AddParameter("hash", hash);
            var result = await client.ExecuteAsync(request);
            return "";
        }
    }

    public static partial class Extensions
    {
        public static async Task<RestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse) await taskCompletion.Task;
        }
    }
}
