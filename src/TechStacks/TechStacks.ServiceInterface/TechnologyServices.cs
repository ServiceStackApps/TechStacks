using System.Linq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate(ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechnologyServices : Service
    {
        public object Post(Tech request)
        {
            var tech = request.ConvertTo<Technology>();
            var session = SessionAs<AuthUserSession>();
            tech.CreatedBy = session.UserName;
            tech.LastModifiedBy = session.UserName;
            tech.OwnerId = session.UserAuthId;

            // disable explicit approval until we first have a problem
            //if (!session.HasRole(RoleNames.Admin))
            //{
            //    tech.LogoApproved = false;
            //}
            tech.LogoApproved = true; 
            
            var id = Db.Insert(tech, selectIdentity: true);
            var createdTechStack = Db.SingleById<Technology>(id);
            return new TechResponse
            {
                Tech = createdTechStack
            };
        }

        public object Put(Tech request)
        {
            var existingTech = Db.SingleById<Technology>(request.Id);
            if (existingTech == null)
                throw HttpError.NotFound("Tech not found");

            var session = SessionAs<AuthUserSession>();
            
            // disable explicit approval until we first have a problem
            //if (request.LogoUrl != existingTech.LogoUrl && !session.HasRole(RoleNames.Admin))
            //{
            //    existingTech.LogoApproved = false;
            //}
            if (existingTech.IsLocked && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("Technology changes are currently restricted to Administrators only.");

            var updated = request.ConvertTo<Technology>();
            //Carry over current logo approved status and locked status
            updated.LogoApproved = existingTech.LogoApproved;
            updated.IsLocked = existingTech.IsLocked;

            updated.LastModifiedBy = session.UserName;
            updated.OwnerId = existingTech.OwnerId;
            updated.CreatedBy = existingTech.CreatedBy;
            Db.Save(updated);

            return new TechResponse
            {
                Tech = updated
            };
        }

        public object Delete(Tech request)
        {
            var existingTech = Db.SingleById<Technology>(request.Id);
            if (existingTech == null)
                throw HttpError.NotFound("Tech not found");

            var session = SessionAs<AuthUserSession>();
            if (existingTech.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this technology.");

            Db.DeleteById<Technology>(request.Id);

            return new TechResponse
            {
                Tech = new Technology { Id = (long)request.Id }
            };
        }

        public object Get(Tech request)
        {
            if (request.Id == null)
            {
                return new TechResponse
                {
                    Techs = Db.Select(Db.From<Technology>().Take(100)).ToList()
                };
            }

            var alreadyExists = Db.Exists<Technology>(x => x.Id == request.Id);
            if (!alreadyExists)
                HttpError.NotFound("Tech stack not found");

            return new TechResponse
            {
                Tech = Db.SingleById<Technology>(request.Id)
            };
        }

        public object Get(GetStacksThatUseTech request)
        {
            var stacksByTech = Db.Select(Db.From<TechnologyStack>()
                .Join<TechnologyChoice, TechnologyStack>(
                    (techChoice, techStack) => techChoice.TechnologyId == request.Id && techChoice.TechnologyStackId == techStack.Id)
                .SelectDistinct(x => x.Id));

            return new GetStacksThatUseTechResponse
            {
                TechStacks = stacksByTech.ToList()
            };
        }
    }
}
