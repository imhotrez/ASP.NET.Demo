using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using Demo.Models.Dto;
using Demo.WebAPI.Services.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using FileProviderService = Demo.WebAPI.Services.DataAccess.FileProviderService;

namespace Demo.WebAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadTestController : ControllerBase {
        private readonly FileProviderService fileProviderService;
        private readonly JsonWebTokenService jsonWebTokenService;

        public FileUploadTestController(FileProviderService fileProviderService,
            JsonWebTokenService jsonWebTokenService) {
            this.fileProviderService = fileProviderService;
            this.jsonWebTokenService = jsonWebTokenService;
        }

        [Route("upload")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Upload([FromForm] UploadImageDto uploadImage,
            CancellationToken cancellationToken) {

            if (!ModelState.IsValid) {
                throw new Exception("Модель загружаемой картинки не валидна");
            }
            
            if (!HttpContext.Request.Headers.TryGetValue("authorization", out var jwt)) {
                throw new Exception("Не удалось получить токен безопасности");
            }

            var stringUserId = jsonWebTokenService.GetClaimValue(jwt.ToString()[7..], "UserId");
            if (!long.TryParse(stringUserId, out var userId)) {
                throw new Exception("Не удалось определить идентификатор пользователя");
            }

            var result = await fileProviderService.SaveFile(uploadImage, userId, cancellationToken);
            return Ok(result);
        }
    }
}