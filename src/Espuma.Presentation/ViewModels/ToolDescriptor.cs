using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public enum ToolKind
    {
        Merge,
        ExtractRange,
        DeletePages,
        NumberPages,
        ImageToPdf,
        PdfToImage,
        Organize
    }

    public sealed record ToolDescriptor(
        ToolKind Kind,
        string Title,
        string Description,
        string IconKey,
        FileKind InputKind,
        bool AllowMultiple)
    {
        public static IReadOnlyList<ToolDescriptor> All { get; } =
        [
            new(ToolKind.Merge, "Unir PDF", "Combina varios PDF en un solo archivo.", "Merge", FileKind.Pdf, true),
            new(ToolKind.Organize, "Organizar PDF", "Reordena y rota páginas arrastrando miniaturas.", "Organize", FileKind.Pdf, false),
            new(ToolKind.ExtractRange, "Extraer rango", "Extrae un rango de páginas a un nuevo PDF.", "Extract", FileKind.Pdf, false),
            new(ToolKind.DeletePages, "Eliminar páginas", "Quita las páginas que no necesitas.", "Delete", FileKind.Pdf, false),
            new(ToolKind.NumberPages, "Numerar páginas", "Agrega el número de página en cada hoja.", "Number", FileKind.Pdf, false),
            new(ToolKind.ImageToPdf, "Imagen a PDF", "Convierte una o varias imágenes en un PDF.", "ImageToPdf", FileKind.Image, true),
            new(ToolKind.PdfToImage, "PDF a imagen", "Exporta cada página como imagen.", "PdfToImage", FileKind.Pdf, false)
        ];
    }
}
