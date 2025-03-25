using API.Requests;
using FluentValidation;

namespace API.Validators
{
    public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");

            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Task Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Status)
                .GreaterThanOrEqualTo(0).WithMessage("Status should be between 0 and 2")
                .InclusiveBetween(0, 2).WithMessage("Status must be between 0 and 2");
        }
    }
}
