using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

using BlazBooruCommon.Data;

namespace BlazBooru.Services
{
    public class BooruAPIService
    {
        private HttpClient Client;

        public IGrouping<string, BooruTagData>[] Tags;
        public string BaseAddress;

        public BooruAPIService()
        {
            Client = new HttpClient();
            BaseAddress = "http://localhost:5000/";
        }
        
        public async Task<BooruImageAPI[]> GetRecentAsync()
        {
            var response = await Client.GetFromJsonAsync<BooruImageAPI[]>($"{BaseAddress}api/posts");
            return response;
        }

        public async Task<BooruImageAPI[]> SearchTagsAsync(string[] tags)
        {
            var Tags = string.Join('+', tags);
            var response = await Client.GetFromJsonAsync<BooruImageAPI[]>($"{BaseAddress}api/posts/search/{Tags}");

            return response;
        }

        public async Task<BooruImageAPI> GetPostByIDAsync(string PostID)
        {
            Console.WriteLine($"{BaseAddress}api/posts/{PostID}");
            var response = await Client.GetFromJsonAsync<BooruImageAPI>($"{BaseAddress}api/posts/{PostID}");

            return response;
        }

        public event Action TagsUpdated;

        public void SetTags(IGrouping<string, BooruTagData>[] Tags)
        {
            this.Tags = Tags;
            TagsSet();
        }

        public void ClearTags()
        {
            Tags = null;
            TagsSet();
        }

        private void TagsSet() => TagsUpdated?.Invoke();
    }
}