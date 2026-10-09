using CommunityToolkit.Mvvm.ComponentModel;
using Espuma.Application.Common;
using Espuma.Presentation.Services;
using Espuma.Presentation.Thumbnails;

namespace Espuma.Presentation.ViewModels
{
    /// <summary>Base for tools that work on the pages of a single PDF.</summary>
    public abstract partial class SinglePdfToolViewModel(ToolDescriptor descriptor, AppServices services)
        : ToolViewModel(descriptor, services)
    {
        private int _loadVersion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasDocument), nameof(Pages), nameof(PageCount), nameof(Summary))]
        public partial PdfPreview? Preview { get; private set; }

        [ObservableProperty]
        public partial bool IsLoading { get; private set; }

        public bool HasDocument => Preview is not null;
        public IReadOnlyList<PageThumbnailViewModel> Pages => Preview?.Pages ?? [];
        public int PageCount => Preview?.PageCount ?? 0;
        public SelectedFile? File => Files.FirstOrDefault();
        public string Summary => File is null ? "" : $"{File.Name} · {PageCount} {(PageCount == 1 ? "página" : "páginas")}";

        protected override Task LoadDetailsAsync(SelectedFile file) => Task.CompletedTask;

        protected override async void OnFilesChanged()
        {
            var version = ++_loadVersion;
            var previous = Preview;
            Preview = null;
            previous?.Dispose();
            OnPropertyChanged(nameof(File));
            OnDocumentChanged();

            if (File is not { } file)
                return;

            IsLoading = true;
            try
            {
                var preview = await PdfPreview.OpenAsync(file.File, Services.Renderer);
                if (version != _loadVersion)
                {
                    preview.Dispose();
                    return;
                }

                Preview = preview;
                OnDocumentLoaded(preview);
                OnDocumentChanged();
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                var title = ex is PdfOperationException pdfError ? ErrorMessages.TitleFor(pdfError.Code) : "No se pudo abrir el PDF";
                Services.Notifications.Error(title, ex.Message);
                Files.Clear();
            }
            finally
            {
                if (version == _loadVersion)
                    IsLoading = false;
            }
        }

        protected virtual void OnDocumentLoaded(PdfPreview preview)
        {
        }

        /// <summary>Called after a document is loaded or unloaded, to refresh commands and texts.</summary>
        protected virtual void OnDocumentChanged()
        {
        }

        protected Task<Stream> OpenInputAsync() => File!.File.OpenReadAsync();
    }
}
