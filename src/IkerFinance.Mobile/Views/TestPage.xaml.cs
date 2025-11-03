using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class TestPage : ContentPage
{
    public TestPage(TestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BaseViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
