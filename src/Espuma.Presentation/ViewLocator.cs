using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Espuma.Presentation.ViewModels;
using Espuma.Presentation.Views;

namespace Espuma.Presentation
{
    /// <summary>Maps each view model to its view explicitly, without reflection.</summary>
    public sealed class ViewLocator : IDataTemplate
    {
        public Control Build(object? data) => data switch
        {
            HomeViewModel => new HomeView(),
            MergeViewModel => new MergeView(),
            ExtractRangeViewModel => new ExtractRangeView(),
            DeletePagesViewModel => new DeletePagesView(),
            NumberPagesViewModel => new NumberPagesView(),
            ImageToPdfViewModel => new ImageToPdfView(),
            PdfToImageViewModel => new PdfToImageView(),
            PlaceholderToolViewModel => new PlaceholderToolView(),
            _ => new TextBlock { Text = $"Vista no encontrada: {data?.GetType().Name}" }
        };

        public bool Match(object? data) => data is ViewModelBase;
    }
}
