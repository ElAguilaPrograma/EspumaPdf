using Avalonia.Controls;
using Avalonia.Input;
using Espuma.Presentation.Thumbnails;
using Espuma.Presentation.ViewModels;

namespace Espuma.Presentation.Views
{
    public partial class NumberPagesView : UserControl
    {
        public NumberPagesView()
        {
            InitializeComponent();
            PageList.AddHandler(TappedEvent, (_, e) =>
            {
                if (ThumbnailHost.FindPage(e.Source) is { } page && DataContext is NumberPagesViewModel viewModel)
                    viewModel.SelectPage(page);
            });
        }
    }
}
