using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace IkerFinance.Mobile.ViewModels;

public partial class AddTransactionViewModel : BaseViewModel
{
    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private string currencyCode = "USD";

    [ObservableProperty]
    private string categoryName = string.Empty;

    [ObservableProperty]
    private DateTime date = DateTime.Now;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private bool isIncome;

    [ObservableProperty]
    private string transactionType = "Expense";

    public AddTransactionViewModel()
    {
        Title = "Add Transaction";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Description))
        {
            SetError("Description is required");
            await Shell.Current.DisplayAlert("Validation Error", "Please enter a description", "OK");
            return;
        }

        if (Amount <= 0)
        {
            SetError("Amount must be greater than zero");
            await Shell.Current.DisplayAlert("Validation Error", "Please enter a valid amount", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            SetError("Category is required");
            await Shell.Current.DisplayAlert("Validation Error", "Please select a category", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            // TODO: Call API to save transaction
            // For now, just simulate a delay
            await Task.Delay(1000);

            // Show success and navigate back
            await Shell.Current.DisplayAlert("Success", "Transaction added successfully", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SetError($"Failed to save transaction: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Failed to save transaction", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    partial void OnTransactionTypeChanged(string value)
    {
        IsIncome = value == "Income";
    }
}
