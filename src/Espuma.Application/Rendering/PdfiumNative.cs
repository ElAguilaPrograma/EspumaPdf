using System.Runtime.InteropServices;

namespace Espuma.Application.Rendering
{
    internal static unsafe partial class PdfiumNative
    {
        private const string Library = "pdfium";

        public const int BitmapFormatBgra = 4;
        public const int RenderAnnotations = 0x01;

        public const uint ErrorFile = 2;
        public const uint ErrorFormat = 3;
        public const uint ErrorPassword = 4;
        public const uint ErrorSecurity = 5;

        [StructLayout(LayoutKind.Sequential)]
        public struct FileAccess
        {
            public CULong FileLength;
            public delegate* unmanaged[Cdecl]<nint, CULong, byte*, CULong, int> GetBlock;
            public nint Param;
        }

        [LibraryImport(Library, EntryPoint = "FPDF_InitLibrary")]
        public static partial void InitLibrary();

        [LibraryImport(Library, EntryPoint = "FPDF_LoadCustomDocument", StringMarshalling = StringMarshalling.Utf8)]
        public static partial nint LoadCustomDocument(FileAccess* access, string? password);

        [LibraryImport(Library, EntryPoint = "FPDF_CloseDocument")]
        public static partial void CloseDocument(nint document);

        [LibraryImport(Library, EntryPoint = "FPDF_GetPageCount")]
        public static partial int GetPageCount(nint document);

        [LibraryImport(Library, EntryPoint = "FPDF_GetLastError")]
        public static partial CULong GetLastError();

        [LibraryImport(Library, EntryPoint = "FPDF_LoadPage")]
        public static partial nint LoadPage(nint document, int index);

        [LibraryImport(Library, EntryPoint = "FPDF_ClosePage")]
        public static partial void ClosePage(nint page);

        [LibraryImport(Library, EntryPoint = "FPDF_GetPageWidthF")]
        public static partial float GetPageWidth(nint page);

        [LibraryImport(Library, EntryPoint = "FPDF_GetPageHeightF")]
        public static partial float GetPageHeight(nint page);

        [LibraryImport(Library, EntryPoint = "FPDFBitmap_CreateEx")]
        public static partial nint BitmapCreateEx(int width, int height, int format, nint buffer, int stride);

        [LibraryImport(Library, EntryPoint = "FPDFBitmap_FillRect")]
        public static partial int BitmapFillRect(nint bitmap, int left, int top, int width, int height, uint color);

        [LibraryImport(Library, EntryPoint = "FPDFBitmap_Destroy")]
        public static partial void BitmapDestroy(nint bitmap);

        [LibraryImport(Library, EntryPoint = "FPDF_RenderPageBitmap")]
        public static partial void RenderPageBitmap(nint bitmap, nint page, int startX, int startY,
            int width, int height, int rotate, int flags);
    }
}
