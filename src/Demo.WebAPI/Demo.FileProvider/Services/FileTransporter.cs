using System;
using System.IO;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using Grpc.Core;
using System.Threading;
using Google.Protobuf;
using FileInfo = Demo.gRPC.FileTransport.FileInfo;

namespace Demo.FileProvider.Services {
    public class FileTransporter : FileTransportService.FileTransportServiceBase {
        private readonly ImageResizer imageResizer;
        private readonly FileStorage fileStorage;

        public FileTransporter(ImageResizer imageResizer, FileStorage fileStorage) {
            this.imageResizer = imageResizer;
            this.fileStorage = fileStorage;
        }

        public override async Task<UploadResult> FileUpload(IAsyncStreamReader<BytesContentUpload> requestStream,
            ServerCallContext context) {
            var result = false;
            while (await requestStream.MoveNext()) {
                var image = requestStream.Current;

                await using var ms = new MemoryStream();
                image.Content.WriteTo(ms);
                var previewBody = imageResizer.Resize(ms);
                var originalBody = ms.ToArray();
                result = await fileStorage.Save(image.DatabaseId, originalBody, previewBody, CancellationToken.None);
            }

            return new UploadResult {UploadCompletedSuccessfully = result};
        }

        public override async Task FileDownload(FileInfo request,
            IServerStreamWriter<BytesContentDownload> responseStream, ServerCallContext context) {
            var result = request.Type switch {
                ImageType.Original => await fileStorage.GetOriginal(request.FileId, CancellationToken.None),
                ImageType.Preview => await fileStorage.GetPreview(request.FileId, CancellationToken.None),
                _ => throw new ArgumentOutOfRangeException()
            };

            await responseStream.WriteAsync(new BytesContentDownload {Content = ByteString.CopyFrom(result)});
        }
    }
}