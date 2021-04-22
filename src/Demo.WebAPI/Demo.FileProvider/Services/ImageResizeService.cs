using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Demo.gRPC.FileTransport;
using SkiaSharp;
using Grpc.Core;

namespace Demo.FileProvider.Services {
    public class ImageResizeService : FileTransportService.FileTransportServiceBase {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="quality"></param>
        /// https://schwabencode.com/blog/2019/06/11/Resize-Image-NET-Core
        /// <returns></returns>
        public (byte[] FileContents, int Height, int Width) Resize(byte[] fileContents, int maxWidth, int maxHeight, SKFilterQuality quality = SKFilterQuality.Medium) {
            using var ms = new MemoryStream(fileContents);
            using var sourceBitmap = SKBitmap.Decode(ms);

            var height = Math.Min(maxHeight, sourceBitmap.Height);
            var width = Math.Min(maxWidth, sourceBitmap.Width);

            using var scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), quality);
            using var scaledImage = SKImage.FromBitmap(scaledBitmap);
            using var data = scaledImage.Encode();

            return (data.ToArray(), height, width);
        }

        public byte[] Resize(MemoryStream memoryStream, int maxWidth, int maxHeight)
        {
            using var sourceBitmap = SKBitmap.Decode(memoryStream);
            var height = Math.Min(maxHeight, sourceBitmap.Height);
            var width = Math.Min(maxWidth, sourceBitmap.Width);
            using var scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            using var scaledImage = SKImage.FromBitmap(scaledBitmap);
            using var data = scaledImage.Encode();
            return data.ToArray();
        }

        public override async Task<Empty> FileUpLoad(IAsyncStreamReader<BytesContent> requestStream, ServerCallContext context) {
            //var contentList = new List<BytesContent>();
            var res = requestStream.ReadAllAsync(CancellationToken.None);

            while (await requestStream.MoveNext()) {
                var image = requestStream.Current;
                await using var ms = new MemoryStream();
                image.Content.WriteTo(ms);
                //var previewBody = Resize(ms, 640, 480);
                var originalBody = ms.ToArray();
            }

            return new Empty();
        }
    }
}
