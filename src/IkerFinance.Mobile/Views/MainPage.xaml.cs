using IkerFinance.Mobile.ViewModels;

namespace IkerFinance.Mobile.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = App.Current.Handler.MauiContext.Services.GetService<MainViewModel>();
    }
}
