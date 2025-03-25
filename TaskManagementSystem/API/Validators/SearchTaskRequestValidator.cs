using API.Requests;
using FluentValidation;

namespace API.Validators
{
    public class SearchTaskRequestValidator : AbstractValidator<SearchTaskRequest>
    {
        public SearchTaskRequestValidator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .NotEmpty().WithMessage("Page size is required.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(0).WithMessage("Page count must be at least 0.")
                .NotEmpty().WithMessage("Page count is required.");

            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.Status)
                .GreaterThanOrEqualTo(0).WithMessage("Status must be a positive integer.")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.AssignedTo)
                .MaximumLength(50).WithMessage("AssignedTo must not exceed 50 characters.");
        }
    }
}
