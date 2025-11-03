using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IkerFinance.Infrastructure.Mobile.Services;
using System.Collections.ObjectModel;

namespace IkerFinance.Mobile.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IMobileAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _selectedCurrency = "USD";

    [ObservableProperty]
    private ObservableCollection<string> _availableCurrencies = new();

    public RegisterViewModel(IMobileAuthService authService)
    {
        _authService = authService;
        Title = "Register";

        AvailableCurrencies = new ObservableCollection<string>
        {
            "USD", "EUR", "SEK", "NGN", "BDT", "CNY"
        };
    }

    [RelayCommand]
    private async Task RegisterAsync()
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

            if (!IsValidEmail(Email))
            {
                SetError("Please enter a valid email address");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Please enter a password");
                return;
            }

            if (Password.Length < 6)
            {
                SetError("Password must be at least 6 characters");
                return;
            }

            if (Password != ConfirmPassword)
            {
                SetError("Passwords do not match");
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                SetError("Please enter your first name");
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                SetError("Please enter your last name");
                return;
            }

            // Attempt registration
            var (success, user, errorMessage) = await _authService.RegisterAsync(
                Email,
                Password,
                FirstName,
                LastName,
                SelectedCurrency);

            if (!success || user == null)
            {
                SetError(errorMessage ?? "Registration failed");
                return;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            SetError($"Registration failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
