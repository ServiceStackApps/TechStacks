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
    public class TechStackFilters
    {
        public static void FilterTechStackRequest(IRequest req, IResponse res, TechStack dto)
        {
            var dbFactory = req.TryResolve<IDbConnectionFactory>();

            if (req.Verb == "POST")
            {
                using (var db = dbFactory.OpenDbConnection())
                {
                    //Check unqiue name
                    var exists = db.Single<TechnologyStack>(x => x.Name.ToLower() == dto.Name.ToLower());

                    if (exists != null)
                    {
                        throw HttpError.Conflict("A TechnologyStack with that name already exists");
                    }
                }
            }
        }
    }
}
