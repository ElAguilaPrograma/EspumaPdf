using SkiaSharp;

namespace Espuma.Application.Common
{
    internal static class ImageInput
    {
        private const int JpegQuality = 92;

        /// <summary>
        /// Returns a stream PDFsharp can import: the original when it is an upright JPEG or PNG, otherwise a
        /// temporary transcoded copy with the EXIF orientation applied.
        /// </summary>
        public static Stream PrepareForPdf(Stream image, out bool ownsStream)
        {
            var seekable = PdfInput.EnsureSeekable(image, out var ownsSeekable);
            try
            {
                seekable.Position = 0;
                SKEncodedImageFormat format;
                SKEncodedOrigin origin;
                SKBitmap? decoded = null;

                using (var managed = new SKManagedStream(seekable, disposeManagedStream: false))
                using (var codec = SKCodec.Create(managed))
                {
                    if (codec is null)
                        throw new PdfOperationException(PdfErrorCode.InvalidImage, "El archivo no es una imagen válida.");

                    format = codec.EncodedFormat;
                    origin = codec.EncodedOrigin;

                    if (format is SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png && origin == SKEncodedOrigin.TopLeft)
                    {
                        seekable.Position = 0;
                        ownsStream = ownsSeekable;
                        return seekable;
                    }

                    decoded = SKBitmap.Decode(codec)
                        ?? throw new PdfOperationException(PdfErrorCode.InvalidImage, "No se pudo leer la imagen.");
                }

                using (decoded)
                using (var oriented = ApplyOrigin(decoded, origin))
                {
                    var output = PdfInput.CreateTempFile();
                    var target = format == SKEncodedImageFormat.Jpeg ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png;
                    (oriented ?? decoded).Encode(output, target, JpegQuality);
                    output.Position = 0;

                    if (ownsSeekable) seekable.Dispose();
                    ownsStream = true;
                    return output;
                }
            }
            catch
            {
                if (ownsSeekable) seekable.Dispose();
                throw;
            }
        }

        /// <summary>Returns a new upright bitmap, or null when the origin is already upright.</summary>
        private static SKBitmap? ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
        {
            if (origin == SKEncodedOrigin.TopLeft)
                return null;

            float w = source.Width, h = source.Height;
            var swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
                or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            var result = swapsAxes
                ? new SKBitmap(source.Height, source.Width, source.ColorType, source.AlphaType)
                : new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);

            using var canvas = new SKCanvas(result);
            switch (origin)
            {
                case SKEncodedOrigin.TopRight:
                    canvas.Translate(w, 0);
                    canvas.Scale(-1, 1);
                    break;
                case SKEncodedOrigin.BottomRight:
                    canvas.Translate(w, h);
                    canvas.RotateDegrees(180);
                    break;
                case SKEncodedOrigin.BottomLeft:
                    canvas.Translate(0, h);
                    canvas.Scale(1, -1);
                    break;
                case SKEncodedOrigin.LeftTop:
                    canvas.Scale(-1, 1);
                    canvas.RotateDegrees(90);
                    break;
                case SKEncodedOrigin.RightTop:
                    canvas.Translate(h, 0);
                    canvas.RotateDegrees(90);
                    break;
                case SKEncodedOrigin.RightBottom:
                    canvas.Translate(h, w);
                    canvas.Scale(1, -1);
                    canvas.RotateDegrees(90);
                    break;
                case SKEncodedOrigin.LeftBottom:
                    canvas.Translate(0, w);
                    canvas.RotateDegrees(-90);
                    break;
            }

            canvas.DrawBitmap(source, 0, 0);
            return result;
        }
    }
}
