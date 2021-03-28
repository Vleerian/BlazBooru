using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components.Forms;

using LiteDB;

using BlazBooruCommon.Data;
using BlazBooruAPI.Services;

namespace BlazBooruAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : Controller
    {
        BooruDataService DataService;

        public PostsController(BooruDataService dataService)
        {
            DataService = dataService;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<string> Upload()
        {
            var file = Request.Form.Files[0];

            var buffer = new byte[file.Length];
            await file.OpenReadStream().ReadAsync(buffer);

            var FileData = await DataService.UploadFile(new BooruUploadedFile()
            {
                Filename = file.FileName,
                Content_Type = file.ContentType,
                Data = buffer
            });

            string ImageMD5;
            using(var md5 = MD5.Create())
            {
                var Hash = md5.ComputeHash(buffer);
                ImageMD5 = string.Join("", Hash.Select(B => B.ToString("X2")));
            }

            var tmp = ((string)Request.Form["tags"]).Split("+");
            var Tags = tmp.Select(t => new BooruTagData() { Type = "general", Tag = t, Refs = 1 });

            var PostID = await DataService.AddPost(new BooruImageAPI
            {
                Image = FileData.ID.ToString(),
                MD5 = ImageMD5,
                Original_Name = file.FileName,
                Tags = Tags.ToArray()
            });

            return PostID.ToString();
        }

        //GET /api/posts/
        [HttpGet]
        public async Task<BooruImageAPI[]> GetRecent() =>
            Array.ConvertAll(await DataService.GetRecent(), i => (BooruImageAPI)i);

        //GET /api/posts/{PostID}
        [HttpGet("{PostID}")]
        public async Task<BooruImageAPI> GetPost(string PostID)
        {
            var ImageData = await DataService.GetPostByID(PostID);
            if(ImageData == null)
            {
                return new BooruImageAPI {
                    ID = int.Parse(PostID),
                    Image = "Not Found"
                };
            }
            return ImageData;
        }

        //GET api/posts/search/{tags}
        [HttpGet("search/{tags}")]
        public async Task<BooruImageAPI[]> SearchTags(string tags) =>
            Array.ConvertAll(await DataService.SearchTags(tags.Split("+")), i => (BooruImageAPI)i);
    }
}