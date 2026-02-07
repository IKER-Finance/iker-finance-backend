using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class BudgetsPage : ContentPage
{
    public BudgetsPage(BudgetsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
