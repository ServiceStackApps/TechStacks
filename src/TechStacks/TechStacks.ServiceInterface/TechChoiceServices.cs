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
        public object Get(TechChoices request)
        {
            if (request.Id == null)
            {
                return new TechChoicesResponse
                {
                    TechnologyChoices = Db.Select<TechnologyChoice>()
                };
            }
            return new TechChoicesResponse
            {
                TechnologyChoice = Db.Single(Db.From<TechnologyChoice>()
                    .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                    .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId)
                    .Where(technologyChoice => technologyChoice.Id == request.Id))
            };
        }

        public object Post(TechChoices request)
        {
            var techChoice = request.ConvertTo<TechnologyChoice>();
            var session = SessionAs<AuthUserSession>();
            techChoice.CreatedBy = session.UserName;
            techChoice.LastModifiedBy = session.UserName;
            techChoice.OwnerId = session.UserAuthId;
            var id = Db.Insert(techChoice, selectIdentity: true);
            var createdTechStack = Db.SingleById<TechnologyChoice>(id);

            return new TechChoicesResponse
            {
                TechnologyChoice = createdTechStack
            };
        }

        public object Put(TechChoices request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
            {
                throw HttpError.NotFound("Techstack technology not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }
            var updated = request.ConvertTo<TechnologyChoice>();
            updated.LastModifiedBy = session.UserName;
            updated.OwnerId = techChoice.OwnerId;
            updated.CreatedBy = techChoice.CreatedBy;
            Db.Save(updated);

            return new TechChoicesResponse
            {
                TechnologyChoice = updated
            };
        }

        public object Delete(TechChoices request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
            {
                throw HttpError.NotFound("Techstack technology not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }
            Db.DeleteById<TechnologyChoice>(request.Id);
            return new TechChoicesResponse
            {
                TechnologyChoice = new TechnologyChoice { Id = (long)request.Id }
            };
        }
    }
}
