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
    public class TechStackValidator : AbstractValidator<ServiceModel.TechStacks>
    {
        public TechStackValidator()
        {
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.Name).NotEmpty();
                //http://stackoverflow.com/a/3831442/670151
                RuleFor(x => x.Name).Matches("(?!^\\d+$)^.+$");
            });
        }
    }
}
