using Avalonia.Platform.Storage;

namespace Espuma.Presentation.Services
{
    public enum FileKind
    {
        Pdf,
        Image,
        Zip
    }

    public static class FileKinds
    {
        private static readonly string[] PdfExtensions = [".pdf"];
        private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif"];
        private static readonly string[] ZipExtensions = [".zip"];

        private static readonly FilePickerFileType PdfType = new("Documentos PDF")
        {
            Patterns = ["*.pdf"],
            MimeTypes = ["application/pdf"],
            AppleUniformTypeIdentifiers = ["com.adobe.pdf"]
        };

        private static readonly FilePickerFileType ImageType = new("Imágenes")
        {
            Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp", "*.bmp", "*.gif"],
            MimeTypes = ["image/*"],
            AppleUniformTypeIdentifiers = ["public.image"]
        };

        private static readonly FilePickerFileType ZipType = new("Archivo ZIP")
        {
            Patterns = ["*.zip"],
            MimeTypes = ["application/zip"],
            AppleUniformTypeIdentifiers = ["public.zip-archive"]
        };

        public static FilePickerFileType PickerType(this FileKind kind) => kind switch
        {
            FileKind.Pdf => PdfType,
            FileKind.Image => ImageType,
            _ => ZipType
        };

        public static string Extension(this FileKind kind) => kind switch
        {
            FileKind.Pdf => ".pdf",
            FileKind.Image => ".png",
            _ => ".zip"
        };

        public static bool Matches(this FileKind kind, string fileName)
        {
            var extensions = kind switch
            {
                FileKind.Pdf => PdfExtensions,
                FileKind.Image => ImageExtensions,
                _ => ZipExtensions
            };
            return extensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
        }
    }
}
