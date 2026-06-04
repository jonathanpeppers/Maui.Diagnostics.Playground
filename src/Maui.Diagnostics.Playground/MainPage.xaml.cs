using Maui.Diagnostics.Playground.Features.Gallery;

namespace Maui.Diagnostics.Playground;

public partial class MainPage : ContentPage
{
	public MainPage(GalleryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
