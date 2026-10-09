using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;
using Espuma.Presentation.Thumbnails;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class DeletePagesViewModel(ToolDescriptor descriptor, AppServices services)
        : SinglePdfToolViewModel(descriptor, services)
    {
        private PageThumbnailViewModel? _anchor;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ActionText), nameof(SelectionText), nameof(HasSelection), nameof(DeletesEverything))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        public partial int SelectedCount { get; private set; }

        public bool HasSelection => SelectedCount > 0;
        public bool DeletesEverything => HasDocument && SelectedCount == PageCount;

        public string ActionText => SelectedCount switch
        {
            0 => "Eliminar páginas",
            1 => "Eliminar 1 página",
            var n => $"Eliminar {n} páginas"
        };

        public string SelectionText => SelectedCount switch
        {
            0 => "Haz clic en las páginas que quieres quitar. Usa Shift para marcar varias seguidas.",
            1 => "1 página marcada",
            var n => $"{n} páginas marcadas"
        };

        private bool CanDelete => SelectedCount > 0 && SelectedCount < PageCount;

        protected override void OnDocumentChanged()
        {
            _anchor = null;
            SelectedCount = 0;
            DeleteCommand.NotifyCanExecuteChanged();
        }

        /// <summary>A click toggles the page; with Shift it marks every page from the last clicked one.</summary>
        public void TogglePage(PageThumbnailViewModel page, bool extend)
        {
            if (extend && _anchor is not null)
            {
                var from = Math.Min(_anchor.PageIndex, page.PageIndex);
                var to = Math.Max(_anchor.PageIndex, page.PageIndex);
                for (var i = from; i <= to; i++)
                    Pages[i].IsSelected = true;
            }
            else
            {
                page.IsSelected = !page.IsSelected;
            }

            _anchor = page;
            SelectedCount = Pages.Count(p => p.IsSelected);
        }

        [RelayCommand]
        private void ClearSelection()
        {
            foreach (var page in Pages)
                page.IsSelected = false;

            _anchor = null;
            SelectedCount = 0;
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private Task DeleteAsync()
        {
            var toDelete = Pages.Where(page => page.IsSelected).Select(page => page.PageIndex).ToList();

            return SaveResultAsync(FileKind.Pdf, $"{BaseName(File!.Name)}-editado.pdf", "Eliminando páginas…",
                [File.File], async (output, _, cancellationToken) =>
                {
                    await using var input = await OpenInputAsync();
                    await Services.PageArrange.DeletePagesAsync(input, toDelete, output, cancellationToken);
                });
        }
    }
}
