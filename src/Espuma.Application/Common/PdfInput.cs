using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Common
{
    internal static class PdfInput
    {
        private const int HeaderSearchLength = 1024;
        private static ReadOnlySpan<byte> PdfHeader => "%PDF-"u8;

        /// <summary>
        /// Returns a seekable view of the stream. Non-seekable streams are buffered to a temporary file
        /// that is deleted when the returned stream is disposed.
        /// </summary>
        public static Stream EnsureSeekable(Stream stream, out bool ownsStream)
        {
            if (stream.CanSeek)
            {
                ownsStream = false;
                return stream;
            }

            var temp = CreateTempFile();
            stream.CopyTo(temp);
            temp.Position = 0;
            ownsStream = true;
            return temp;
        }

        /// <summary>Temporary file deleted when the stream is disposed.</summary>
        public static FileStream CreateTempFile() =>
            new(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None,
                bufferSize: 81920, FileOptions.DeleteOnClose);

        public static void ValidatePdfHeader(Stream stream) => FindHeaderOffset(stream);

        /// <summary>Position of "%PDF-", which the standard allows anywhere in the first kilobyte.</summary>
        private static long FindHeaderOffset(Stream stream)
        {
            if (stream.Length == 0)
                throw new PdfOperationException(PdfErrorCode.InvalidPdf, "El archivo está vacío.");

            var originalPosition = stream.Position;
            Span<byte> buffer = stackalloc byte[HeaderSearchLength];
            stream.Position = 0;
            var read = stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
            stream.Position = originalPosition;

            var offset = buffer[..read].IndexOf(PdfHeader);
            if (offset < 0)
                throw new PdfOperationException(PdfErrorCode.InvalidPdf, "El archivo no es un PDF válido.");

            return offset;
        }

        public static PdfDocument Open(Stream stream, PdfDocumentOpenMode mode)
        {
            var headerOffset = FindHeaderOffset(stream);
            var source = headerOffset == 0 ? stream : new OffsetStream(stream, headerOffset);
            source.Position = 0;

            var passwordRequested = false;
            PdfDocument? document;
            try
            {
                document = PdfReader.Open(source, mode, args =>
                {
                    passwordRequested = true;
                    args.Abort = true;
                });
            }
            catch (Exception ex) when (ex is not PdfOperationException)
            {
                if (passwordRequested)
                    throw Encrypted(ex);

                throw new PdfOperationException(PdfErrorCode.Corrupted, "El PDF está dañado o no se puede leer.", ex);
            }

            return document ?? throw Encrypted(null);
        }

        private static PdfOperationException Encrypted(Exception? inner) =>
            new(PdfErrorCode.Encrypted, "El PDF está protegido con contraseña.", inner);
    }
}
