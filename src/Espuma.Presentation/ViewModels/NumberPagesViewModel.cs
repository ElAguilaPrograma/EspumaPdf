using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Application.Services;
using Espuma.Presentation.Services;
using Espuma.Presentation.Thumbnails;

namespace Espuma.Presentation.ViewModels
{
    public sealed record NumberFormatOption(string Label, string Format);

    public sealed partial class NumberPagesViewModel : SinglePdfToolViewModel
    {
        public NumberPagesViewModel(ToolDescriptor descriptor, AppServices services) : base(descriptor, services)
        {
            SelectedFormat = Formats[0];
        }

        public IReadOnlyList<NumberFormatOption> Formats { get; } =
        [
            new("1 | 10", "{0} | {1}"),
            new("1 de 10", "{0} de {1}"),
            new("Página 1 de 10", "Página {0} de {1}"),
            new("1", "{0}")
        ];

        [ObservableProperty]
        public partial decimal? FirstPage { get; set; } = 1;

        [ObservableProperty]
        public partial NumberFormatOption SelectedFormat { get; set; }

        private bool CanNumber => HasDocument && FirstPage is >= 1 && FirstPage <= PageCount;

        partial void OnFirstPageChanged(decimal? value) => UpdateBadges();

        partial void OnSelectedFormatChanged(NumberFormatOption value) => UpdateBadges();

        protected override void OnDocumentLoaded(PdfPreview preview) => FirstPage = 1;

        protected override void OnDocumentChanged() => UpdateBadges();

        /// <summary>Clicking a page makes numbering start there.</summary>
        public void SelectPage(PageThumbnailViewModel page) => FirstPage = page.PageNumber;

        private void UpdateBadges()
        {
            var first = CanNumber ? (int)FirstPage!.Value : int.MaxValue;
            foreach (var page in Pages)
                page.Badge = page.PageNumber >= first ? string.Format(SelectedFormat.Format, page.PageNumber, PageCount) : null;

            NumberCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanNumber))]
        private Task NumberAsync()
        {
            var options = new PageNumberOptions((int)FirstPage!.Value - 1, SelectedFormat.Format);

            return SaveResultAsync(FileKind.Pdf, $"{BaseName(File!.Name)}-numerado.pdf", "Numerando páginas…",
                [File.File], async (output, progress, cancellationToken) =>
                {
                    await using var input = await OpenInputAsync();
                    await Services.PageNumbers.AddPageNumbersAsync(input, output, options, progress, cancellationToken);
                });
        }
    }
}
