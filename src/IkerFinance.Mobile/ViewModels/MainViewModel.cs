using CommunityToolkit.Mvvm.Input;
using IkerFinance.Infrastructure.Mobile.Services;

namespace IkerFinance.Mobile.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IMobileAuthService _authService;

    public MainViewModel(IMobileAuthService authService)
    {
        _authService = authService;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            await _authService.LogoutAsync();

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            SetError($"Logout failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
