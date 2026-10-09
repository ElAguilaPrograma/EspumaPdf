using Avalonia.Controls;
using Avalonia.Input;
using Espuma.Presentation.Thumbnails;
using Espuma.Presentation.ViewModels;

namespace Espuma.Presentation.Views
{
    public partial class ExtractRangeView : UserControl
    {
        public ExtractRangeView()
        {
            InitializeComponent();
            PageList.AddHandler(TappedEvent, (_, e) =>
            {
                if (ThumbnailHost.FindPage(e.Source) is { } page && DataContext is ExtractRangeViewModel viewModel)
                    viewModel.SelectPage(page, e.KeyModifiers.HasFlag(KeyModifiers.Shift));
            });
        }
    }
}
