using Avalonia.Controls;
using Avalonia.Input;
using Espuma.Presentation.Thumbnails;
using Espuma.Presentation.ViewModels;

namespace Espuma.Presentation.Views
{
    public partial class DeletePagesView : UserControl
    {
        public DeletePagesView()
        {
            InitializeComponent();
            PageList.AddHandler(TappedEvent, (_, e) =>
            {
                if (ThumbnailHost.FindPage(e.Source) is { } page && DataContext is DeletePagesViewModel viewModel)
                    viewModel.TogglePage(page, e.KeyModifiers.HasFlag(KeyModifiers.Shift));
            });
        }
    }
}
