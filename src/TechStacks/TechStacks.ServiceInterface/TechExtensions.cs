using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    public static class TechExtensions
    {
        public static void PopulateTechTiers(this TechStackDetails techStackDetails,
            List<TechnologyChoice> techs)
        {
            techStackDetails.TechnologyChoices = techs.Select(MergeTechnologyInformation).ToList();
        }

        public static TechnologyInStack MergeTechnologyInformation(TechnologyChoice technologyChoice)
        {
            var result = technologyChoice.ConvertTo<TechnologyInStack>();
            result.PopulateWith(technologyChoice.Technology);
            result.Id = technologyChoice.Id;
            return result;
        }
    }
}
