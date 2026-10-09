using Espuma.Application.Rendering;
using Espuma.Application.Services;

namespace Espuma.Presentation.Services
{
    /// <summary>Everything the view models need, built once at startup.</summary>
    public sealed class AppServices
    {
        public AppServices(IFileDialogService fileDialogs, Func<Uri, Task<bool>> openUri,
            Func<IOutputFile, Task<bool>> openFile)
        {
            FileDialogs = fileDialogs;
            OpenUri = openUri;
            OpenFile = openFile;
            Runner = new OperationRunner(Notifications);
            PdfToImage = new PdfToImageService(Renderer);
        }

        public NotificationService Notifications { get; } = new();
        public OperationRunner Runner { get; }
        public IFileDialogService FileDialogs { get; }
        public Func<Uri, Task<bool>> OpenUri { get; }
        public Func<IOutputFile, Task<bool>> OpenFile { get; }

        public IPdfRenderer Renderer { get; } = new PdfiumRenderer();
        public IPdfMergeService Merge { get; } = new PdfMergeService();
        public IPdfPageArrangeService PageArrange { get; } = new PdfPageArrangeService();
        public IPdfPageNumberService PageNumbers { get; } = new PdfPageNumberService();
        public IPdfImageToPdfService ImageToPdf { get; } = new PdfImageToPdfService();
        public IPdfToImageService PdfToImage { get; }
    }
}
