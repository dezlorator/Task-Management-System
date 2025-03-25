using API.Requests;
using FluentValidation;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Task Name is required")
            .MaximumLength(100).WithMessage("Task Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 2).WithMessage("Status must be between 0 and 2");

        RuleFor(x => x.AssignedTo)
            .MaximumLength(50).WithMessage("AssignedTo cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.AssignedTo));
    }
}
