using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
