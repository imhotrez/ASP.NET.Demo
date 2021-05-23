﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using Demo.gRPC.SPA.FileTransport;
using Demo.WebAPI.Services.DataAccess;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using BytesContentDownload = Demo.gRPC.SPA.FileTransport.BytesContentDownload;

namespace Demo.WebAPI.Services.BusinessLogic {
    public class ImageTransport : ImageTransportService.ImageTransportServiceBase {
        private readonly FileProviderService fileProviderService;
        private readonly DemoContext dbContext;

        public ImageTransport(FileProviderService fileProviderService, DemoContext dbContext) {
            this.fileProviderService = fileProviderService;
            this.dbContext = dbContext;
        }

        public override async Task FileDownload(UserInfo request,
            IServerStreamWriter<BytesContentDownload> responseStream,
            ServerCallContext context) {
            var imagesIds = await this.dbContext.Users
                .Include(u => u.ImageMetadatas)
                .SelectMany(u => u.ImageMetadatas.Select(x => x.Id))
                .ToArrayAsync(CancellationToken.None);

            var imageType = request.IsPreview ? ImageType.Preview : ImageType.Original;
            foreach (var imageId in imagesIds) {
                var result = await fileProviderService.DownloadFile(imageId, imageType, CancellationToken.None);
                await responseStream.WriteAsync(new BytesContentDownload {
                    Content = ByteString.CopyFrom(result.Body),
                    FileName = result.Name,
                    IsPreview = request.IsPreview,
                    FileId = imageId
                });
            }
        }
    }
}