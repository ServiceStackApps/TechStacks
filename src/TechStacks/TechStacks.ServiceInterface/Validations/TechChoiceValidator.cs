using ServiceStack;
using ServiceStack.FluentValidation;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface.Validations
{
    public class TechChoiceValidator : AbstractValidator<CreateTechChoice>
    {
        public TechChoiceValidator()
        {
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.Tier).NotNull();
            });
        }
    }
}