using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        Console.WriteLine("=== LoginPage constructor called ===");
        Console.WriteLine($"LoginPage: ViewModel is {(viewModel == null ? "NULL" : "NOT NULL")}");
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Console.WriteLine($"LoginPage: BindingContext set to {(BindingContext == null ? "NULL" : BindingContext.GetType().Name)}");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
