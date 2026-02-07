using IkerFinance.Mobile.Views;

namespace IkerFinance.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
		Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
		Routing.RegisterRoute(nameof(TransactionsPage), typeof(TransactionsPage));
		Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
		Routing.RegisterRoute(nameof(BudgetsPage), typeof(BudgetsPage));
		Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
	}
}
