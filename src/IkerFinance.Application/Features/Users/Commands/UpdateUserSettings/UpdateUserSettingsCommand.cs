using IkerFinance.Application.DTOs.Users;
using MediatR;

namespace IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommand : IRequest<UserSettingsDto>
{
    public string UserId { get; set; } = string.Empty;
    public int? DefaultTransactionCurrencyId { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}
