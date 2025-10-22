using FluentAssertions;
using Moq;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Application.Features.Feedbacks.Queries.GetFeedbacks;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Application.Features.Feedbacks.Queries;

public class GetFeedbacksQueryHandlerTests
{
    private readonly Mock<IFeedbackRepository> _mockRepo;
    private readonly GetFeedbacksQueryHandler _handler;

    public GetFeedbacksQueryHandlerTests()
    {
        _mockRepo = new Mock<IFeedbackRepository>();
        _handler = new GetFeedbacksQueryHandler(_mockRepo.Object);
    }

    // ✅ Test 1: 有效查询返回分页数据
    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnExpectedData()
    {
        var query = new GetFeedbacksQuery
        {
            Type = FeedbackType.Bug,
            Status = FeedbackStatus.Open,
            Priority = FeedbackPriority.High,
            PageNumber = 1,
            PageSize = 10
        };

        var expectedResponse = new PaginatedResponse<FeedbackDto>
        {
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            Data = new List<FeedbackDto>
            {
                new FeedbackDto
                {
                    Id = 1,
                    Subject = "Critical Bug",
                    Description = "Crashes on login",
                    Type = FeedbackType.Bug,
                    Status = FeedbackStatus.Open
                }
            }
        };

        _mockRepo
            .Setup(x => x.GetFeedbacksAsync(It.IsAny<FeedbackFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.Data.Should().ContainSingle();
        result.Data.First().Subject.Should().Be("Critical Bug");

        _mockRepo.Verify(x => x.GetFeedbacksAsync(It.IsAny<FeedbackFilters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ✅ Test 2: 空结果时返回空 Data 集合
    [Fact]
    public async Task Handle_WhenNoFeedbacks_ShouldReturnEmptyResult()
    {
        var query = new GetFeedbacksQuery { PageNumber = 1, PageSize = 10 };

        _mockRepo
            .Setup(x => x.GetFeedbacksAsync(It.IsAny<FeedbackFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResponse<FeedbackDto>
            {
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                Data = new List<FeedbackDto>()
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Data.Should().BeEmpty();
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    // ✅ Test 3: 确认查询过滤条件正确传入仓储层
    [Fact]
    public async Task Handle_ShouldPassCorrectFiltersToRepository()
    {
        FeedbackFilters? capturedFilters = null;

        _mockRepo
            .Setup(x => x.GetFeedbacksAsync(It.IsAny<FeedbackFilters>(), It.IsAny<CancellationToken>()))
            .Callback<FeedbackFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(new PaginatedResponse<FeedbackDto>());

        var query = new GetFeedbacksQuery
        {
            Type = FeedbackType.Improvement,
            Status = FeedbackStatus.Resolved,
            Priority = FeedbackPriority.Medium,
            PageNumber = 2,
            PageSize = 5
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.Type.Should().Be(FeedbackType.Improvement);
        capturedFilters.Status.Should().Be(FeedbackStatus.Resolved);
        capturedFilters.Priority.Should().Be(FeedbackPriority.Medium);
    }
}
