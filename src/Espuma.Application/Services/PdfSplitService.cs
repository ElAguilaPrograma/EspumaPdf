using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    public interface IPdfSplitService
    {
        byte[] ExtractPages(Stream file, int starPage, int endPage);
    }
    public class PdfSplitService : IPdfSplitService
    {
        private readonly InterfaceIsPdf _validatePdf;
        public PdfSplitService(InterfaceIsPdf interfaceIsPdf)
        {
            _validatePdf = interfaceIsPdf;
        }

        public byte[] ExtractPages(Stream file, int startPage, int endPage)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No se proporcionó el archivo PDF");

            if (!_validatePdf.ValidatePdf(file))
                throw new InvalidDataException("Uno de los archivos no es un PDF valido");

            using var inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            if (startPage < 1)
                throw new InvalidDataException("Pagina de inicia, no valida");

            if (endPage > inputDocument.PageCount)
                throw new InvalidDataException($"Rango invalido, este pdf solo tiene {inputDocument.PageCount} paginas");

            if (endPage < startPage)
                throw new InvalidDataException("La pagina final no puede ser menor a la inicial");

            try
            {
                for (int i = startPage - 1; i < endPage; i++)
                    outputDocument.AddPage(inputDocument.Pages[i]);
            }
            catch(PdfReaderException)
            {
                throw new InvalidDataException("El archivo PDf esta dañado o corrupto.");
            }

            using var stream  = new MemoryStream();
            outputDocument.Save(stream);
            return stream.ToArray();
        }

    }
}
