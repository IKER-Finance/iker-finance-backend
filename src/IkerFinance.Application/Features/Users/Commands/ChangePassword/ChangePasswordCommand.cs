using MediatR;

namespace IkerFinance.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
