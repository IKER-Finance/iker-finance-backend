using FluentValidation;

namespace IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;

public class UpdateFeedbackStatusCommandValidator : AbstractValidator<UpdateFeedbackStatusCommand>
{
    public UpdateFeedbackStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Feedback ID is required");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid status");

        RuleFor(x => x.AdminResponse)
            .MaximumLength(2000)
            .WithMessage("Admin response cannot exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AdminResponse));
    }
}
