using Avalonia;
using Avalonia.Controls;
using Espuma.Presentation.ViewModels;

namespace Espuma.Presentation.Views
{
    public partial class MainView : UserControl
    {
        private const double CompactSidebarWidth = 860;

        public MainView() => InitializeComponent();

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            if (DataContext is MainViewModel viewModel)
                viewModel.IsSidebarCompact = e.NewSize.Width < CompactSidebarWidth;
        }
    }
}
