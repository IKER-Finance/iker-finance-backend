using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IkerFinance.Infrastructure.Mobile.Services;

namespace IkerFinance.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IMobileAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = true;

    public LoginViewModel(IMobileAuthService authService)
    {
        _authService = authService;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            // Validate inputs
            if (string.IsNullOrWhiteSpace(Email))
            {
                SetError("Please enter your email");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Please enter your password");
                return;
            }

            // Attempt login
            var (success, user, errorMessage) = await _authService.LoginAsync(Email, Password);

            if (!success || user == null)
            {
                SetError(errorMessage ?? "Login failed");
                return;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }
}
