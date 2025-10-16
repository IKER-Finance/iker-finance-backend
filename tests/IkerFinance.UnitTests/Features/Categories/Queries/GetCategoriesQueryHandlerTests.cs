using FluentAssertions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Categories.Queries.GetCategories;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Categories.Queries;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetCategoriesQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetCategoriesQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetCategoriesQueryHandler(_mockContext.Object);
    }

    // Test: Returns both system and user-specific active categories
    [Fact]
    public async Task Handle_ReturnsSystemAndUserCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Food", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 2, Name = "Transport", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 2 },
            new() { Id = 3, Name = "Custom", Type = TransactionType.Expense, IsSystem = false, UserId = TestUserId, IsActive = true, SortOrder = 1 },
            new() { Id = 4, Name = "Other User", Type = TransactionType.Expense, IsSystem = false, UserId = "otherUser", IsActive = true, SortOrder = 1 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
        result.Should().Contain(c => c.Id == 3);
        result.Should().NotContain(c => c.Id == 4);
    }

    // Test: Returns only active categories
    [Fact]
    public async Task Handle_ReturnsOnlyActiveCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Active", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 2, Name = "Inactive", Type = TransactionType.Expense, IsSystem = true, IsActive = false, SortOrder = 2 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    // Test: Orders by SortOrder, then Name
    [Fact]
    public async Task Handle_OrdersBySortOrderThenName()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Food", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 2 },
            new() { Id = 2, Name = "Bills", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 3, Name = "Transport", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 4, Name = "Shopping", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 3 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result[0].Name.Should().Be("Bills");
        result[1].Name.Should().Be("Transport");
        result[2].Name.Should().Be("Food");
        result[3].Name.Should().Be("Shopping");
    }

    // Test: Maps category properties correctly to DTO
    [Fact]
    public async Task Handle_MapsCategoryPropertiesToDto()
    {
        var categories = new List<Category>
        {
            new()
            {
                Id = 5,
                Name = "Shopping",
                Description = "Shopping expenses",
                Color = "#FF5733",
                Icon = "cart",
                Type = TransactionType.Expense,
                IsSystem = false,
                UserId = TestUserId,
                IsActive = true,
                SortOrder = 1
            }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(5);
        result[0].Name.Should().Be("Shopping");
        result[0].Description.Should().Be("Shopping expenses");
        result[0].Color.Should().Be("#FF5733");
        result[0].Icon.Should().Be("cart");
        result[0].Type.Should().Be(TransactionType.Expense);
        result[0].IsSystem.Should().BeFalse();
        result[0].IsActive.Should().BeTrue();
    }

    // Test: Returns empty list when no categories match criteria
    [Fact]
    public async Task Handle_WhenNoCategoriesMatch_ReturnsEmptyList()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Inactive", Type = TransactionType.Expense, IsSystem = true, IsActive = false, SortOrder = 1 },
            new() { Id = 2, Name = "Other User", Type = TransactionType.Expense, IsSystem = false, UserId = "otherUser", IsActive = true, SortOrder = 1 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // Test: Returns only system categories when user has no custom categories
    [Fact]
    public async Task Handle_WhenUserHasNoCustomCategories_ReturnsOnlySystemCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Food", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 2, Name = "Transport", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.IsSystem);
    }

    // Test: Handles categories with same sort order by name
    [Fact]
    public async Task Handle_WithSameSortOrder_OrdersByName()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Zebra", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 2, Name = "Apple", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new() { Id = 3, Name = "Mango", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 }
        };

        var mockDbSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.Categories).Returns(mockDbSet.Object);

        var query = new GetCategoriesQuery { UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result[0].Name.Should().Be("Apple");
        result[1].Name.Should().Be("Mango");
        result[2].Name.Should().Be("Zebra");
    }

    private static Mock<DbSet<Category>> CreateMockDbSet(List<Category> data)
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}
