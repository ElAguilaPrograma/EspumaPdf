using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;

namespace Espuma.Application.Services
{
    public interface IPdfToImageService
    {
        byte[] ConvertPdfToImages(Stream pdfStream);
    }

    public class PdfToImageService : IPdfToImageService
    {
        private readonly InterfaceIsPdf _validatePdf;

        public PdfToImageService(InterfaceIsPdf interfaceIsPdf)
        {
            _validatePdf = interfaceIsPdf;
        }

        public byte[] ConvertPdfToImages(Stream pdfStream)
        {
            if (pdfStream == null || pdfStream.Length == 0)
                throw new ArgumentException("No se proporciono el PDF");

            if (!_validatePdf.ValidatePdf(pdfStream))
                throw new InvalidDataException("Archivo PDF no valido");

            pdfStream.Position = 0;
            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                pdfStream.CopyTo(ms);
                pdfBytes = ms.ToArray();
            }

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                using var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(2.0d)); // 2.0d scaling factor for better resolution
                
                for (int i = 0; i < docReader.GetPageCount(); i++)
                {
                    using var pageReader = docReader.GetPageReader(i);
                    var rawBytes = pageReader.GetImage(); // BGRA
                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();

                    using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
                    
                    using var imageStream = new MemoryStream();
                    image.SaveAsPng(imageStream);

                    var entry = archive.CreateEntry($"page_{i + 1}.png");
                    using var entryStream = entry.Open();
                    entryStream.Write(imageStream.ToArray());
                }
            }

            return zipStream.ToArray();
        }
    }
}