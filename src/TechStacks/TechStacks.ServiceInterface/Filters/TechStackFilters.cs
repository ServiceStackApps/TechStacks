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
        public static void FilterCreateTechStackRequest(IRequest req, IResponse res, CreateTechnologyStack dto)
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

        public static void FilterUpdateTechStackRequest(IRequest req, IResponse res, UpdateTechnologyStack dto)
        {
            var dbFactory = req.TryResolve<IDbConnectionFactory>();
            if (req.Verb == "PUT")
            {
                using (var db = dbFactory.OpenDbConnection())
                {
                    //Check unqiue name
                    var exists = db.Single<TechnologyStack>(x => x.Name.ToLower() == dto.Name.ToLower());
                    if (exists != null && exists.Id != dto.Id)
                    {
                        throw HttpError.Conflict("A TechnologyStack with that name already exists");
                    }
                }
            }
        }
    }
}
