using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate(ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechChoiceServices : Service
    {
        public object Get(FindTechChoices request)
        {
            var q = Db.From<TechnologyChoice>()
                .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId);

            if (request.TechnologyId != null)
                q.Where(x => x.TechnologyStackId == request.TechnologyId);

            if (request.TechnologyStackId != null)
                q.Where(x => x.TechnologyStackId == request.TechnologyStackId);

            return new TechChoicesResponse
            {
                Results = Db.Select(q)
            };
        }

        public object Get(GetTechChoice request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
                throw HttpError.NotFound("TechChoice not found");

            return new TechChoiceResponse
            {
                Result = techChoice
            };
        }

        public object Post(CreateTechChoice request)
        {
            var tech = Db.SingleById<Technology>(request.TechnologyId);
            if (tech == null)
                throw HttpError.NotFound("Tech not found");

            var tierExists = tech.Tier == request.Tier;
            if (!tierExists)
                throw HttpError.NotFound("Invalid Tier with technology choice");

            var stackFound = Db.Exists<TechnologyStack>(x => x.Id == request.TechnologyStackId);
            if (!stackFound)
                throw HttpError.NotFound("Techstack not found");

            var techChoiceAlreadyExists = Db.Exists<TechnologyChoice>(
                x => x.TechnologyId == request.TechnologyId && x.TechnologyStackId == request.TechnologyStackId);
            if (techChoiceAlreadyExists)
                throw HttpError.Conflict("Tech choice already exists");

            var techChoice = request.ConvertTo<TechnologyChoice>();
            var session = SessionAs<AuthUserSession>();
            techChoice.CreatedBy = session.UserName;
            techChoice.LastModifiedBy = session.UserName;
            techChoice.OwnerId = session.UserAuthId;
            var id = Db.Insert(techChoice, selectIdentity: true);
            var createdTechStack = Db.SingleById<TechnologyChoice>(id);

            return new TechChoiceResponse
            {
                Result = createdTechStack
            };
        }

        public object Put(UpdateTechChoice request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
                throw HttpError.NotFound("Techstack technology not found");

            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this stack.");

            var updated = request.ConvertTo<TechnologyChoice>();
            updated.LastModifiedBy = session.UserName;
            updated.OwnerId = techChoice.OwnerId;
            updated.CreatedBy = techChoice.CreatedBy;
            Db.Save(updated);

            return new TechChoiceResponse
            {
                Result = updated
            };
        }

        public object Delete(DeleteTechChoice request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
                throw HttpError.NotFound("Techstack technology not found");

            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            
            Db.DeleteById<TechnologyChoice>(request.Id);

            return new TechChoiceResponse
            {
                Result = new TechnologyChoice { Id = request.Id }
            };
        }
    }
}
