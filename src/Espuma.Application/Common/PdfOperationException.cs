namespace Espuma.Application.Common
{
    public sealed class PdfOperationException : Exception
    {
        public PdfOperationException(PdfErrorCode code, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            Code = code;
        }

        public PdfErrorCode Code { get; }
    }
}
