using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using IkerFinance.Application;
using IkerFinance.Infrastructure.Mobile;
using IkerFinance.Infrastructure.Mobile.Data;
using IkerFinance.Mobile.ViewModels;
using IkerFinance.Mobile.Views;

namespace IkerFinance.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddApplication();

		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ikerfinance.db");
		builder.Services.AddMobileInfrastructure(dbPath);

		builder.Services.AddTransient<TestViewModel>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<TransactionsViewModel>();
		builder.Services.AddTransient<AddTransactionViewModel>();
		builder.Services.AddTransient<BudgetsViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();

		builder.Services.AddTransient<TestPage>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<TransactionsPage>();
		builder.Services.AddTransient<AddTransactionPage>();
		builder.Services.AddTransient<BudgetsPage>();
		builder.Services.AddTransient<ProfilePage>();

		var app = builder.Build();

		InitializeDatabaseAsync(app.Services).GetAwaiter().GetResult();

		return app;
	}

	private static async Task InitializeDatabaseAsync(IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<MobileDbContext>();
		await DbInitializer.InitializeAsync(context);
	}
}
