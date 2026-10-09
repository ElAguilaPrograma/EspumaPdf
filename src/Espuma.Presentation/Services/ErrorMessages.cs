using Espuma.Application.Common;

namespace Espuma.Presentation.Services
{
    public static class ErrorMessages
    {
        public static string TitleFor(PdfErrorCode code) => code switch
        {
            PdfErrorCode.InvalidPdf => "No es un PDF válido",
            PdfErrorCode.Corrupted => "PDF dañado",
            PdfErrorCode.Encrypted => "PDF protegido",
            PdfErrorCode.InvalidImage => "Imagen no válida",
            PdfErrorCode.InvalidRange => "Rango no válido",
            PdfErrorCode.NotEnoughFiles => "Faltan archivos",
            PdfErrorCode.NoPagesSelected => "Sin páginas",
            _ => "Algo salió mal"
        };
    }
}
