using IkerFinance.Application.DTOs.Users;
using MediatR;

namespace IkerFinance.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQuery : IRequest<UserProfileDto>
{
    public string UserId { get; set; } = string.Empty;
}
