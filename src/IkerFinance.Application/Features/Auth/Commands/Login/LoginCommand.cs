using MediatR;
using IkerFinance.Application.DTOs.Auth;

namespace IkerFinance.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;