using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    public interface IPdfDeletePagesService
    {
        byte[] DeletePages(Stream file, List<int> pagesToDelete);
    }
    public class PdfDeletePagesService : IPdfDeletePagesService
    {
        private readonly InterfaceIsPdf _validatePdf;
        public PdfDeletePagesService (InterfaceIsPdf interfaceIsPdf)
        {
            _validatePdf = interfaceIsPdf;
        }

        public byte[] DeletePages(Stream file, List<int> pagesToDelete)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No se proporcionó el archivo PDF");

            if (!_validatePdf.ValidatePdf(file))
                throw new InvalidDataException("Uno de los archivos no es un PDF valido");

            using var inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            // Se pasa la lista a un hashset para busqueda rapida
            var deleteSet = new HashSet<int>(pagesToDelete);

            try
            {
                for (int i = 0; i < inputDocument.PageCount; i++)
                {
                    if (!deleteSet.Contains(i + 1))
                        outputDocument.AddPage(inputDocument.Pages[i]);
                }
            }
            catch (PdfReaderException)
            {
                throw new InvalidDataException("El archivo PDf esta dañado o corrupto.");
            }

            using var stream = new MemoryStream();
            outputDocument.Save(stream);
            return stream.ToArray();
        }
    }
}
