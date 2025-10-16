using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Infrastructure.Data;

namespace IkerFinance.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<FeedbackDto>> GetFeedbacksAsync(
        FeedbackFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Feedbacks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            query = query.Where(f =>
                f.Subject.ToLower().Contains(searchTerm) ||
                f.Description.ToLower().Contains(searchTerm));
        }

        if (filters.Type.HasValue)
            query = query.Where(f => f.Type == filters.Type.Value);

        if (filters.Status.HasValue)
            query = query.Where(f => f.Status == filters.Status.Value);

        if (filters.Priority.HasValue)
            query = query.Where(f => f.Priority == filters.Priority.Value);

        if (filters.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(filters.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(f => f.CreatedAt >= startDateUtc);
        }

        if (filters.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(filters.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(f => f.CreatedAt <= endDateUtc);
        }

        // Sorting
        query = filters.SortBy.ToLower() switch
        {
            "createdat" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(f => f.CreatedAt)
                : query.OrderByDescending(f => f.CreatedAt),
            "status" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(f => f.Status)
                : query.OrderByDescending(f => f.Status),
            "priority" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(f => f.Priority)
                : query.OrderByDescending(f => f.Priority),
            _ => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(f => f.CreatedAt)
                : query.OrderByDescending(f => f.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var feedbacks = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Join(
                _context.Users,
                f => f.UserId,
                u => u.Id,
                (f, u) => new { Feedback = f, User = u }
            )
            .GroupJoin(
                _context.Users,
                fu => fu.Feedback.RespondedByUserId,
                ru => ru.Id,
                (fu, respondedUsers) => new { fu.Feedback, fu.User, RespondedBy = respondedUsers.FirstOrDefault() }
            )
            .Select(x => new FeedbackDto
            {
                Id = x.Feedback.Id,
                UserId = x.Feedback.UserId,
                UserName = x.User.FirstName + " " + x.User.LastName,
                UserEmail = x.User.Email!,
                Type = x.Feedback.Type,
                Subject = x.Feedback.Subject,
                Description = x.Feedback.Description,
                Priority = x.Feedback.Priority,
                Status = x.Feedback.Status,
                AdminResponse = x.Feedback.AdminResponse,
                RespondedByUserId = x.Feedback.RespondedByUserId,
                RespondedByUserName = x.RespondedBy != null ? x.RespondedBy.FirstName + " " + x.RespondedBy.LastName : null,
                ResponseDate = x.Feedback.ResponseDate,
                CreatedAt = x.Feedback.CreatedAt,
                UpdatedAt = x.Feedback.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<FeedbackDto>
        {
            Data = feedbacks,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }
}
