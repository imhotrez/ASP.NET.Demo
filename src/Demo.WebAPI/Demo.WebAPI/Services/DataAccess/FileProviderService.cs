using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.gRPC.FileTransport;
using Demo.Models.Domain.Image;
using Demo.Models.Dto;
using Demo.Models.Filters;
using Google.Protobuf;
using FileInfo = Demo.gRPC.FileTransport.FileInfo;

namespace Demo.WebAPI.Services.DataAccess {
    public class FileProviderService : BaseDbService<Metadata, Metadata, BaseFilter> {
        private readonly FileTransportService.FileTransportServiceClient fileTransportServiceClient;

        public FileProviderService(DemoContext dbContext, IMapper mapper,
            FileTransportService.FileTransportServiceClient fileTransportServiceClient)
            : base(dbContext, mapper) {
            this.fileTransportServiceClient = fileTransportServiceClient;
        }

        public async Task<bool> SaveFile(UploadImageDto uploadImage, long userId,
            CancellationToken cancellationToken = default) {
            await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            try {
                var metadata = new Metadata {
                    UserId = userId,
                    Name = uploadImage.FileName,
                    NormalizedName = uploadImage.FileName.ToUpperInvariant(),
                };

                metadata = await Save(metadata, null, cancellationToken);

                await using var memoryStream = new MemoryStream();
                await uploadImage.Body.CopyToAsync(memoryStream, cancellationToken);
                var byteString = ByteString.CopyFrom(memoryStream.ToArray());
                var bytesContent = new BytesContentUpload {
                    Content = byteString, DatabaseId = metadata.Id, FileName = uploadImage.FileName
                };

                var fileUpLoadProcess = fileTransportServiceClient.FileUpload(cancellationToken: cancellationToken);
                await fileUpLoadProcess.RequestStream.WriteAsync(bytesContent);
                await fileUpLoadProcess.RequestStream.CompleteAsync();
                var uploadResult = await fileUpLoadProcess;

                await transaction.CommitAsync(cancellationToken);

                return uploadResult.UploadCompletedSuccessfully;
            } catch (Exception) {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<ImageDataDto> DownloadFile(long id, ImageType type, CancellationToken token) {
            var meta = await Get(id, token);
            if (meta == null) {
                throw new Exception($"Файл с идентификатором {id} не найден");
            }

            var call = fileTransportServiceClient.FileDownload(new FileInfo {FileId = id, Type = type},
                cancellationToken: token);
            
            await call.ResponseStream.MoveNext(token);
            var imageData = new ImageDataDto {
                Body = call.ResponseStream.Current.Content.ToByteArray(), 
                Name = meta.Name
            };

            return imageData;
        }
    }
}