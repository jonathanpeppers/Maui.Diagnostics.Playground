using Microsoft.Extensions.DependencyInjection;
using Maui.Diagnostics.Playground.Features.Scenarios;

namespace Maui.Diagnostics.Playground;

public partial class App : Application
{
	private readonly IServiceProvider services;

	public App(IServiceProvider services)
	{
		this.services = services;
		StartupCrashCoordinator.CrashIfArmed();
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(services.GetRequiredService<AppShell>());
	}
}