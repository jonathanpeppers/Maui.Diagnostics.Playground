using Microsoft.Extensions.DependencyInjection;

namespace Maui.Diagnostics.Playground;

public partial class App : Application
{
	private readonly IServiceProvider services;

	public App(IServiceProvider services)
	{
		this.services = services;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(services.GetRequiredService<AppShell>());
	}
}