using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class AddTransactionPage : ContentPage
{
    public AddTransactionPage(AddTransactionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
