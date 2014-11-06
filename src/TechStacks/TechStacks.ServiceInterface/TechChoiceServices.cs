using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate(ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechChoiceServices : Service
    {
        public object Get(TechChoice request)
        {
            if (request.Id == null)
            {
                var technologyChoices = Db.Select<TechnologyChoice>(

                    );
                return new TechChoiceResponse
                {
                    TechnologyChoices =
                        technologyChoices.ToList()
                };
            }
            return new TechChoiceResponse
            {
                TechnologyChoice = Db.Single<TechnologyChoice>(Db.From<TechnologyChoice>()
                        .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                        .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId)
                        .Where(technologyChoice => technologyChoice.Id == request.Id))
            };
        }

        public object Post(TechChoice request)
        {
            var tech = request.ConvertTo<TechnologyChoice>();
            var session = SessionAs<AuthUserSession>();
            tech.CreatedBy = session.UserName;
            tech.LastModifiedBy = session.UserName;
            tech.OwnerId = session.UserAuthId;
            var id = Db.Insert(tech, selectIdentity: true);
            var createdTechStack = Db.SingleById<TechnologyChoice>(id);
            return new TechChoiceResponse
            {
                TechnologyChoice = createdTechStack
            };
        }

        public object Put(TechChoice request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
            {
                throw HttpError.NotFound("Techstack technology not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId)
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }
            var updated = request.ConvertTo<TechnologyChoice>();
            updated.LastModifiedBy = session.UserName;
            updated.OwnerId = techChoice.OwnerId;
            updated.CreatedBy = techChoice.CreatedBy;
            Db.Save(updated);

            return new TechChoiceResponse
            {
                TechnologyChoice = updated
            };
        }

        public object Delete(TechChoice request)
        {
            var techChoice = Db.SingleById<TechnologyChoice>(request.Id);
            if (techChoice == null)
            {
                throw HttpError.NotFound("Techstack technology not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (techChoice.OwnerId != session.UserAuthId)
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }
            Db.DeleteById<TechnologyChoice>(request.Id);
            return new TechChoiceResponse
            {
                TechnologyChoice = new TechnologyChoice { Id = (long)request.Id }
            };
        }
    }
}
