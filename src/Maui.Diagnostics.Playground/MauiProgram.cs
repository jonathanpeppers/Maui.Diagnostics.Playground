using Maui.Diagnostics.Playground.Diagnostics;
using Maui.Diagnostics.Playground.Features.Gallery;
using Maui.Diagnostics.Playground.Features.Scenarios;
using Microsoft.Extensions.Logging;

namespace Maui.Diagnostics.Playground;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddSingleton<ICrashScenarioCatalog, CrashScenarioCatalog>();
		builder.Services.AddSingleton<ICrashScenarioRunner, ManagedCrashScenarioRunner>();
		builder.Services.AddSingleton<IDiagnosticsSelfReportService, DiagnosticsSelfReportService>();
		builder.Services.AddTransient<GalleryViewModel>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<ScenarioDetailPage>();

		return builder.Build();
	}
}
