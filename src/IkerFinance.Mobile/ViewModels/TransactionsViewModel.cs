using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IkerFinance.Mobile.Models;

namespace IkerFinance.Mobile.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
    public ObservableCollection<TransactionModel> Transactions { get; } = new();

    public TransactionsViewModel()
    {
        Title = "Transactions";
        LoadTransactions();
    }

    private void LoadTransactions()
    {
        // TODO: Replace with actual API call
        // For now, adding mock data
        Transactions.Clear();

        Transactions.Add(new TransactionModel
        {
            Id = 1,
            Description = "Grocery Shopping",
            Amount = 125.50m,
            CurrencyCode = "USD",
            CategoryName = "Food",
            Date = DateTime.Now.AddDays(-1),
            IsIncome = false
        });

        Transactions.Add(new TransactionModel
        {
            Id = 2,
            Description = "Salary",
            Amount = 3500.00m,
            CurrencyCode = "USD",
            CategoryName = "Income",
            Date = DateTime.Now.AddDays(-5),
            IsIncome = true
        });

        Transactions.Add(new TransactionModel
        {
            Id = 3,
            Description = "Gas Station",
            Amount = 45.00m,
            CurrencyCode = "USD",
            CategoryName = "Transport",
            Date = DateTime.Now.AddDays(-2),
            IsIncome = false
        });

        Transactions.Add(new TransactionModel
        {
            Id = 4,
            Description = "Netflix Subscription",
            Amount = 15.99m,
            CurrencyCode = "USD",
            CategoryName = "Entertainment",
            Date = DateTime.Now.AddDays(-7),
            IsIncome = false
        });

        Transactions.Add(new TransactionModel
        {
            Id = 5,
            Description = "Coffee Shop",
            Amount = 8.50m,
            CurrencyCode = "USD",
            CategoryName = "Food",
            Date = DateTime.Now,
            IsIncome = false
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await Task.Delay(1000); // Simulate API call
            LoadTransactions();
        }
        catch (Exception ex)
        {
            SetError($"Failed to refresh: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        await Shell.Current.GoToAsync("AddTransactionPage");
    }
}
