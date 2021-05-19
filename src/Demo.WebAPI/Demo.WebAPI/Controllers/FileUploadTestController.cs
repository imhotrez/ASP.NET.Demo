using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using Demo.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FileProviderService = Demo.WebAPI.Services.DataAccess.FileProviderService;

namespace Demo.WebAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadTestController : ControllerBase {
        private readonly FileProviderService fileProviderService;

        public FileUploadTestController(FileProviderService fileProviderService) {
            this.fileProviderService = fileProviderService;
        }

        [Route("upload")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Upload([FromForm] UploadImageDto uploadImage,
            CancellationToken cancellationToken) {
            var state = ModelState;
            // TODO Реализовать получение ID пользователя
            var result = await fileProviderService.SaveFile(uploadImage, 1, cancellationToken);
            return Ok("Good"
                //result
                );
        }

        [Route("download/{id}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Download(long id, CancellationToken cancellationToken) {
            var state = ModelState;
            // TODO Реализовать получение ID пользователя
            var result = await fileProviderService.DownloadFile(id, ImageType.Preview, cancellationToken);
            var stream = new MemoryStream(result.Body);
            return File(stream, "application/octet-stream", result.Name);
        }
    }
}