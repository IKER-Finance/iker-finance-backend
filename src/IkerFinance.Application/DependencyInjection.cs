using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IkerFinance.Application.Features.Auth.Commands.Register;

namespace IkerFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);
        
        return services;
    }
}