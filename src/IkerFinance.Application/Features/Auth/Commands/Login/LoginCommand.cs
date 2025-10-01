using MediatR;
using IkerFinance.Shared.DTOs.Auth;

namespace IkerFinance.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;