using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Demo.FileProvider.Services {
    public class ImageResizer {
        public byte[] Resize(Stream memoryStream) {
            const int size = 150;
            const int quality = 75;

            using var image = new Bitmap(Image.FromStream(memoryStream));
            int width, height;
            if (image.Width > image.Height) {
                width = size;
                height = Convert.ToInt32(image.Height * size / (double) image.Width);
            } else {
                width = Convert.ToInt32(image.Width * size / (double) image.Height);
                height = size;
            }

            var resized = new Bitmap(width, height);

            using var graphics = Graphics.FromImage(resized);
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(image, 0, 0, width, height);
            using var output = new MemoryStream();
            var qualityParamId = Encoder.Quality;
            var encoderParameters = new EncoderParameters(1) {
                Param = {[0] = new EncoderParameter(qualityParamId, quality)}
            };

            var codec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Png.Guid);

            if (codec == null) {
                throw new InvalidOperationException("Не удалось получить объект ImageDecoder");
            }

            resized.Save(output, codec, encoderParameters);
            return output.ToArray();
        }
    }
}