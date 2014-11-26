using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Web;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface.Filters
{
    public class TechChoiceFilters
    {
        public static void FilterTechChoiceRequest(IRequest req, IResponse res, TechChoice dto)
        {
            var dbFactory = req.TryResolve<IDbConnectionFactory>();
            if (req.Verb == "GET")
            {
                bool choiceFound;
                if (dto.Id != null)
                {
                    using (var db = dbFactory.OpenDbConnection())
                    {
                        choiceFound = db.Exists<TechnologyChoice>(x => x.Id == dto.Id);
                    }
                    if (!choiceFound)
                    {
                        HttpError.NotFound("Tech not found");
                    }
                }
            }
            if (req.Verb == "POST")
            {
                bool technologyFound;
                bool stackFound;
                bool techChoiceAlreadyExists;
                bool tierExists;
                using (var db = dbFactory.OpenDbConnection())
                {
                    var tech = db.SingleById<Technology>(dto.TechnologyId);
                    technologyFound = tech != null;
                    tierExists = tech != null && tech.Tiers.Contains(dto.Tier);
                    stackFound = db.Exists<TechnologyStack>(x => x.Id == dto.TechnologyStackId);
                    techChoiceAlreadyExists = db.Exists<TechnologyChoice>(
                        x => x.TechnologyId == dto.TechnologyId && x.TechnologyStackId == dto.TechnologyStackId && x.Tier == dto.Tier);
                }
                if (!stackFound)
                {
                    throw HttpError.NotFound("Techstack not found");
                }
                if (!technologyFound)
                {
                    throw HttpError.NotFound("Tech not found");
                }
                if (techChoiceAlreadyExists)
                {
                    throw HttpError.Conflict("Tech choice already exists");
                }
                if (!tierExists)
                {
                    throw HttpError.NotFound("Invalid Tier with technology choice");
                }
            }
        }
    }
}
