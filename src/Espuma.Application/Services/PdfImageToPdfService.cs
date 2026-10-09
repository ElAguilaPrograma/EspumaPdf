using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Espuma.Application.Services
{
    public interface IPdfImageToPdfService
    {
        byte[] ConvertImagesToPdf(List<Stream> imagesStreams);
        byte[] ConvertImagesToPdfFixedPage(List<Stream> imagesStreams);
        byte[] ConvertImageToPdf(Stream inputImage);
    }
    public class PdfImageToPdfService : IPdfImageToPdfService
    {
        private readonly IIsValidImage _isValidImage;
        public PdfImageToPdfService(IIsValidImage isValidImage)
        {
            _isValidImage = isValidImage;
        }

        public byte[] ConvertImageToPdf(Stream inputImage)
        {
            if (inputImage == null || inputImage.Length == 0)
                throw new ArgumentException("No se proporcionó el archivo PDF");

            if (!_isValidImage.ValidateImage(inputImage))
                throw new InvalidDataException("El archivo no es una image valida");

            using var document = new PdfDocument();
            var page = document.AddPage();

            using var gfx = XGraphics.FromPdfPage(page);
            using var image = XImage.FromStream(inputImage);

            // tamaño de la pagina segun el tamaño de la imagen
            page.Width = XUnit.FromPoint(image.PixelWidth);
            page.Height = XUnit.FromPoint(image.PixelHeight);

            gfx.DrawImage(image, 0, 0, image.PixelWidth, image.PixelHeight);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        public byte[] ConvertImagesToPdf(List<Stream> imagesStreams)
        {
            using var document = new PdfDocument();

            foreach (var imageStream in imagesStreams)
            {
                if (imageStream == null || imageStream.Length == 0)
                    throw new ArgumentException("No se proporcionó el archivo PDF");

                if (!_isValidImage.ValidateImage(imageStream))
                    throw new InvalidDataException("El archivo no es una image valida");

                imageStream.Position = 0;
                var page = document.AddPage();

                using var image = XImage.FromStream(imageStream);

                page.Width = XUnit.FromPoint(image.PixelWidth);
                page.Height = XUnit.FromPoint(image.PixelHeight);

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
            }

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        public byte[] ConvertImagesToPdfFixedPage(List<Stream> imagesStreams)
        {
            if (imagesStreams == null || imagesStreams.Count == 0)
                throw new ArgumentException("No se proporcionaron imagenes");
            using var document = new PdfDocument();

            foreach (var imageStream in imagesStreams)
            {
                if (imageStream == null || imageStream.Length == 0)
                    throw new ArgumentException("No se proporcionó el archivo PDF");

                if (!_isValidImage.ValidateImage(imageStream))
                    throw new InvalidDataException("El archivo no es una image valida");

                imageStream.Position = 0;

                var page = document.AddPage();
                page.Size = PageSize.A4;

                using var gfx = XGraphics.FromPdfPage(page);
                using var image = XImage.FromStream(imageStream);

                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                double imgWidth = image.PixelWidth;
                double imgHeight = image.PixelHeight;

                double scaleX = pageWidth / imgWidth;
                double scaleY = pageHeight / imgHeight;
                double scale = Math.Min(scaleX, scaleY);

                double drawWidth = imgWidth * scale;
                double drawHeight = imgHeight * scale;

                double x = (pageWidth - drawWidth) / 2;
                double y = (pageHeight - drawHeight) / 2;

                gfx.DrawImage(image, x, y, drawWidth, drawHeight);
            }

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }
    }
}
