using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    public interface IPdfTextService
    {
        byte[] AddPageNumbers(Stream file, int startPage,string format = "{0} | {1}");
    }

    public class PdfTextService : IPdfTextService
    {
        private readonly InterfaceIsPdf _validatePdf;
        public PdfTextService(InterfaceIsPdf interfaceIsPdf)
        {
            _validatePdf = interfaceIsPdf;
        }

        // Agregar numero de pagina al pdf.
        public byte[] AddPageNumbers(Stream file, int startPage, string format = "{0} | {1}")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No se proporcionó el archivo PDF");

            if (!_validatePdf.ValidatePdf(file))
                throw new InvalidDataException("Uno de los archivos no es un PDF valido");

            using var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify);
            int totalPages = document.PageCount;

            if (startPage < 0 || startPage > totalPages)
                throw new ArgumentException("La pagina de inicio debe ser mayor a 0 y inferior al numero paginas");

            try
            {
                for (int i = startPage; i < totalPages; i++)
                {
                    var page = document.Pages[i];
                    using var gfx = XGraphics.FromPdfPage(page);

                    var font = new XFont("Roboto", 16);
                    var text = string.Format(format, i + 1, totalPages);
                    var size = gfx.MeasureString(text, font);

                    // Posición
                    double x = (page.Width.Point - size.Width - 35);
                    double y = page.Height.Point - 35;

                    gfx.DrawString(text, font, XBrushes.Black, new XPoint(x, y));
                }
            }
            catch (PdfReaderException)
            {
                throw new InvalidDataException("El archivo PDf esta dañado o corrupto.");
            }

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
    }
}
