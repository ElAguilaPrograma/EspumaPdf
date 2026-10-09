using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Espuma.Presentation.Thumbnails;
using Espuma.Presentation.ViewModels;

namespace Espuma.Presentation.Views
{
    public partial class OrganizeView : UserControl
    {
        public OrganizeView()
        {
            InitializeComponent();
            PageList.AddHandler(TappedEvent, (_, e) =>
            {
                if (e.Source is Control source && source.FindAncestorOfType<Button>(includeSelf: true) is not null)
                    return;

                if (ThumbnailHost.FindPage(e.Source) is not { } page || DataContext is not OrganizeViewModel viewModel)
                    return;

                var toggle = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
                viewModel.SelectPage(page, toggle, e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                Focus();
            });
        }
    }
}
