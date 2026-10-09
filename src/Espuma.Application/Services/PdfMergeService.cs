using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    public interface IPdfMergeService
    {
        byte[] MergePdfsFromStreams(List<Stream> pdfStreams);
    }
    public class PdfMergeService : IPdfMergeService
    {
        private readonly InterfaceIsPdf _validatePdf;
        public PdfMergeService(InterfaceIsPdf interfaceIsPdf)
        {
            _validatePdf = interfaceIsPdf;
        }
        public byte[] MergePdfsFromStreams(List<Stream> pdfStreams)
        {
            if (pdfStreams == null || pdfStreams.Count == 0)
                throw new ArgumentException("No se proporcionaron los archivos PDF");

            if (pdfStreams.Count == 1)
                throw new ArgumentException("Se necesitan al menos dos archivos para poder unirlos");

            using var outputDocument = new PdfDocument();

            foreach (var pdfStream in pdfStreams)
            {
                if (!_validatePdf.ValidatePdf(pdfStream))
                    throw new InvalidDataException("Uno de los archivos no es un PDF valido");

                pdfStream.Position = 0; // Importante: hay que resetear la posición

                try
                {
                    using var inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);

                    for (int i = 0; i < inputDocument.PageCount; i++)
                    {
                        outputDocument.AddPage(inputDocument.Pages[i]);
                    }
                }
                catch
                {
                    throw new InvalidDataException("Uno de los PDFs esta corrupto o no es valido");
                }

            }

            using var stream = new MemoryStream();
            outputDocument.Save(stream, false);
            return stream.ToArray();
        }
    }
}
