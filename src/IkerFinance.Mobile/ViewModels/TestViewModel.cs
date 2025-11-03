using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using IkerFinance.Application.Features.Categories.Queries.GetCategories;

namespace IkerFinance.Mobile.ViewModels;

public partial class TestViewModel : BaseViewModel
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _testMessage = "Testing IkerFinance Mobile";

    [ObservableProperty]
    private int _categoryCount;

    public TestViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Title = "Test Page";
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var query = new GetCategoriesQuery();
            var categories = await _mediator.Send(query);

            CategoryCount = categories.Count();
            TestMessage = $"Successfully loaded {CategoryCount} categories!";
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
