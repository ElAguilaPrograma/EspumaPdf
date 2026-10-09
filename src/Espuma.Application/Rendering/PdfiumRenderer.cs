using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Espuma.Application.Common;

namespace Espuma.Application.Rendering
{
    public sealed class PdfiumRenderer : IPdfRenderer
    {
        /// <summary>PDFium is not thread-safe: every native call goes through this lock.</summary>
        internal static readonly Lock NativeLock = new();
        private static bool _initialized;

        public IPdfRenderDocument Open(Stream pdf)
        {
            lock (NativeLock)
            {
                if (!_initialized)
                {
                    PdfiumNative.InitLibrary();
                    _initialized = true;
                }
            }

            return new PdfiumDocument(pdf);
        }

        private sealed unsafe class PdfiumDocument : IPdfRenderDocument
        {
            private readonly Stream _stream;
            private readonly bool _ownsStream;
            private GCHandle _handle;
            private PdfiumNative.FileAccess* _access;
            private nint _document;

            public PdfiumDocument(Stream pdf)
            {
                _stream = PdfInput.EnsureSeekable(pdf, out _ownsStream);
                try
                {
                    PdfInput.ValidatePdfHeader(_stream);

                    _handle = GCHandle.Alloc(this);
                    _access = (PdfiumNative.FileAccess*)NativeMemory.Alloc((nuint)sizeof(PdfiumNative.FileAccess));
                    _access->FileLength = new CULong((nuint)_stream.Length);
                    _access->GetBlock = &GetBlock;
                    _access->Param = GCHandle.ToIntPtr(_handle);

                    lock (NativeLock)
                    {
                        _document = PdfiumNative.LoadCustomDocument(_access, null);
                        if (_document == 0)
                            throw ToException((uint)PdfiumNative.GetLastError().Value);

                        PageCount = PdfiumNative.GetPageCount(_document);
                    }
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public int PageCount { get; }

            public PageSize GetPageSize(int pageIndex)
            {
                lock (NativeLock)
                {
                    var page = LoadPage(pageIndex);
                    try
                    {
                        return new PageSize(PdfiumNative.GetPageWidth(page), PdfiumNative.GetPageHeight(page));
                    }
                    finally
                    {
                        PdfiumNative.ClosePage(page);
                    }
                }
            }

            public void Render(int pageIndex, nint buffer, int width, int height, int stride,
                PageRotation rotation = PageRotation.None)
            {
                lock (NativeLock)
                {
                    var page = LoadPage(pageIndex);
                    var bitmap = PdfiumNative.BitmapCreateEx(width, height, PdfiumNative.BitmapFormatBgra, buffer, stride);
                    try
                    {
                        PdfiumNative.BitmapFillRect(bitmap, 0, 0, width, height, 0xFFFFFFFF);
                        PdfiumNative.RenderPageBitmap(bitmap, page, 0, 0, width, height, (int)rotation / 90,
                            PdfiumNative.RenderAnnotations);
                    }
                    finally
                    {
                        PdfiumNative.BitmapDestroy(bitmap);
                        PdfiumNative.ClosePage(page);
                    }
                }
            }

            public void Dispose()
            {
                lock (NativeLock)
                {
                    if (_document != 0)
                    {
                        PdfiumNative.CloseDocument(_document);
                        _document = 0;
                    }
                }

                if (_access != null)
                {
                    NativeMemory.Free(_access);
                    _access = null;
                }

                if (_handle.IsAllocated)
                    _handle.Free();

                if (_ownsStream)
                    _stream.Dispose();
            }

            private nint LoadPage(int pageIndex)
            {
                ObjectDisposedException.ThrowIf(_document == 0, this);
                ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(pageIndex, PageCount);

                var page = PdfiumNative.LoadPage(_document, pageIndex);
                return page != 0
                    ? page
                    : throw new PdfOperationException(PdfErrorCode.Corrupted, $"No se pudo leer la página {pageIndex + 1}.");
            }

            private static PdfOperationException ToException(uint error) => error switch
            {
                PdfiumNative.ErrorPassword or PdfiumNative.ErrorSecurity =>
                    new(PdfErrorCode.Encrypted, "El PDF está protegido con contraseña."),
                PdfiumNative.ErrorFormat =>
                    new(PdfErrorCode.Corrupted, "El PDF está dañado o no se puede leer."),
                _ => new(PdfErrorCode.InvalidPdf, $"No se pudo abrir el PDF (error {error}).")
            };

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            private static int GetBlock(nint param, CULong position, byte* buffer, CULong size)
            {
                try
                {
                    var document = (PdfiumDocument)GCHandle.FromIntPtr(param).Target!;
                    document._stream.Position = (long)position.Value;
                    document._stream.ReadExactly(new Span<byte>(buffer, checked((int)size.Value)));
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}
