using MediatR;
using IkerFinance.Application.DTOs.Auth;

namespace IkerFinance.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    int HomeCurrencyId
) : IRequest<AuthResponse>;