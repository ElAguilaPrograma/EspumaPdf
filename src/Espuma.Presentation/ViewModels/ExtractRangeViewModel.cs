using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;
using Espuma.Presentation.Thumbnails;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class ExtractRangeViewModel(ToolDescriptor descriptor, AppServices services)
        : SinglePdfToolViewModel(descriptor, services)
    {
        [ObservableProperty]
        public partial decimal? FirstPage { get; set; }

        [ObservableProperty]
        public partial decimal? LastPage { get; set; }

        private bool IsRangeValid => HasDocument && FirstPage is >= 1 && LastPage is { } last && last <= PageCount && FirstPage <= last;
        private int RangeLength => IsRangeValid ? (int)(LastPage!.Value - FirstPage!.Value) + 1 : 0;

        public string ActionText => RangeLength switch
        {
            0 => "Extraer páginas",
            1 => "Extraer 1 página",
            var n => $"Extraer {n} páginas"
        };

        partial void OnFirstPageChanged(decimal? value) => UpdateRange();

        partial void OnLastPageChanged(decimal? value) => UpdateRange();

        protected override void OnDocumentLoaded(PdfPreview preview)
        {
            FirstPage = 1;
            LastPage = preview.PageCount;
        }

        protected override void OnDocumentChanged() => UpdateRange();

        /// <summary>A click starts a new range on the page; with Shift it extends the current range.</summary>
        public void SelectPage(PageThumbnailViewModel page, bool extend)
        {
            if (extend && FirstPage is { } first)
            {
                FirstPage = Math.Min(first, page.PageNumber);
                LastPage = Math.Max((int)first, page.PageNumber);
                return;
            }

            FirstPage = page.PageNumber;
            LastPage = page.PageNumber;
        }

        private void UpdateRange()
        {
            foreach (var page in Pages)
                page.IsHighlighted = IsRangeValid && page.PageNumber >= FirstPage && page.PageNumber <= LastPage;

            OnPropertyChanged(nameof(ActionText));
            ExtractCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(IsRangeValid))]
        private Task ExtractAsync()
        {
            var first = (int)FirstPage!.Value;
            var last = (int)LastPage!.Value;
            var suffix = first == last ? $"pagina-{first}" : $"paginas-{first}-{last}";

            return SaveResultAsync(FileKind.Pdf, $"{BaseName(File!.Name)}-{suffix}.pdf", "Extrayendo páginas…",
                [File.File], async (output, _, cancellationToken) =>
                {
                    await using var input = await OpenInputAsync();
                    await Services.PageArrange.ExtractRangeAsync(input, first - 1, last - 1, output, cancellationToken);
                });
        }
    }
}
