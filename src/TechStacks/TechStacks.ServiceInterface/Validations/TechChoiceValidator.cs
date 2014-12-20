using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.FluentValidation;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface.Validations
{
    public class TechChoiceValidator : AbstractValidator<ServiceModel.TechChoices>
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