using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using LiteDB;

using BlazBooruCommon.Data;
using BlazBooruAPI.Services;

namespace BlazBooruAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        BooruDataService DataService;

        public FileController(BooruDataService dataService)
        {
            DataService = dataService;
        }

        // GET: api/file/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            var FileData = await DataService.DownloadFile(id);
            return File(FileData.Data, "image/jpeg");
        }

        // GET: api/file/{id}/data
        [HttpGet("{id}/data")]
        public async Task<BooruFileData> GetFileData(string id)
        {
            var FileData = await DataService.DownloadFile(id, true);
            return FileData;
        }
    }
}