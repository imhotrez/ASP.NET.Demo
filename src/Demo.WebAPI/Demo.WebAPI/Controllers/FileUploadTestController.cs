using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Demo.WebAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadTestController : ControllerBase {
        private readonly FileTransportService.FileTransportServiceClient fileTransportServiceClient;

        public FileUploadTestController(FileTransportService.FileTransportServiceClient fileTransportServiceClient) {
            this.fileTransportServiceClient = fileTransportServiceClient;
        }

        [Route("upload")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Upload([FromForm] UploadImage uploadImage, CancellationToken cancellationToken) {
            await using var memoryStream = new MemoryStream();
            await uploadImage.image.CopyToAsync(memoryStream, cancellationToken);
            var byteString = ByteString.CopyFrom(memoryStream.ToArray());
            var bytesContent = new BytesContent {
                Content = byteString,
                Block = 10,
                FileName = "Name"
            };

            var result = fileTransportServiceClient.FileUpLoad();
            await result.RequestStream.WriteAsync(bytesContent);
            await result.RequestStream.CompleteAsync();
            var response = await result;
            return Ok(response);
        }
    }

    public class UploadImage {
        /// <summary>
        /// Тело документа
        /// </summary>
        public IFormFile image { get; set; }
    }
}
