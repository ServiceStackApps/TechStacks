using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    public class TechStackQueries
    {
        public static List<TechStackDetails> GetTechstackDetails(IDbConnection db, SqlExpression<TechnologyStack> stackQuery)
        {
            //distinct
            var latestStacks = db.Select(stackQuery)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            if (latestStacks.Count == 0)
                return new List<TechStackDetails>();

            var technologyChoices =
                db.LoadSelect(db.From<TechnologyChoice>()
                    .Join<Technology>()
                    .Join<TechnologyStack>()
                    .Where(techChoice => 
                        Sql.In(techChoice.TechnologyStackId, latestStacks.Select(x => x.Id))
                    ));

            var stackDetails = latestStacks.Map(x => x.ConvertTo<TechStackDetails>());
            stackDetails.ForEach(stack => stack.TechnologyChoices = technologyChoices
                .Map(x => x.ToTechnologyInStack())
                .Where(x => stack.Id == x.TechnologyStackId)
                .ToList());

            return stackDetails;
        }
    }
}
