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
            var latestStacks = db.Select(stackQuery).ToList()
                //Distinct
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            if (latestStacks.Count == 0)
            {
                return new List<TechStackDetails>();
            }

            var technologyChoices =
                db.LoadSelect(db.From<TechnologyChoice>()
                    .SelectDistinct<TechnologyChoice>(x => x)
                        .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                        .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId)
                        .Where(techChoice => 
                            Sql.In(techChoice.TechnologyStackId, latestStacks.Select(x => x.Id).ToList())
                        ));

            var results = new List<TechStackDetails>();
            latestStacks.ForEach(stack =>
            {
                var techStackDetails = stack.ConvertTo<TechStackDetails>();
                techStackDetails.TechnologyChoices = technologyChoices
                    .Map(x => x.ToTechnologyInStack())
                    .Where(x => stack.Id == x.TechnologyStackId)
                    .ToList();

                results.Add(techStackDetails);
            });
            return results;
        }
    }
}
