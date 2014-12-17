using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownSharp;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate( ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechnologyStackServices : Service
    {
        public object Post(TechStack request)
        {
            var tech = request.ConvertTo<TechnologyStack>();
            var session = SessionAs<AuthUserSession>();
            tech.CreatedBy = session.UserName;
            tech.LastModifiedBy = session.UserName;
            tech.OwnerId = session.UserAuthId;
            tech.Created = DateTime.UtcNow;
            tech.LastModified = DateTime.UtcNow;
            var id = Db.Insert(tech,selectIdentity:true);
            var createdTechStack = Db.SingleById<TechnologyStack>(id);
            return new TechStackResponse
            {
                TechStack = createdTechStack.ConvertTo<TechStackDetails>()
            };
        }

        public object Put(TechStack request)
        {
            var existingStack = Db.SingleById<TechnologyStack>(request.Id);
            if (existingStack == null)
            {
                throw HttpError.NotFound("Tech stack not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (existingStack.OwnerId != session.UserAuthId)
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }
            var updated = request.ConvertTo<TechnologyStack>();

            updated.OwnerId = existingStack.OwnerId;
            updated.LastModifiedBy = session.UserName;
            updated.CreatedBy = existingStack.CreatedBy;
            updated.LastModified = DateTime.UtcNow;
            Db.Save(updated);

            return new TechStackResponse
            {
                TechStack = updated.ConvertTo<TechStackDetails>()
            };
        }

        public object Delete(TechStack request)
        {
            var stack = Db.SingleById<TechnologyStack>(request.Id);
            if (stack == null)
            {
                throw HttpError.NotFound("Tech stack not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (stack.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
            {
                throw HttpError.Unauthorized("You are not the owner of this stack.");
            }

            Db.Delete<TechnologyChoice>(q => q.TechnologyStackId == request.Id);
            Db.DeleteById<TechnologyStack>(request.Id);
            
            return new TechStackResponse
            {
                TechStack = new TechnologyStack { Id = (long)request.Id }.ConvertTo<TechStackDetails>()
            }; 
        }

        public object Get(TechStack request)
        {
            if (request.Id == null)
            {
                return new TechStackResponse
                {
                    TechStacks = Db.Select(Db.From<TechnologyStack>().Take(100)).ToList()
                };
            }
            var alreadyExists = Db.Exists<TechnologyStack>(x => x.Id == request.Id);
            if (!alreadyExists)
            {
                throw HttpError.NotFound("Tech stack not found");
            }
            var response = GetTechnologyStackWithDetails(request);
            return response;
        }

        public object Get(TechStackByTier request)
        {
            var query = Db.From<TechnologyStack>();
            if (!string.IsNullOrEmpty(request.Tier))
            {
                //Filter by tier
                query.Join<TechnologyChoice>((stack, choice) => stack.Id == choice.TechnologyStackId)
                    .Where<TechnologyChoice>(x => Sql.In(x.Tier, request.Tier));
            }

            return new TechStackResponse
            {
                TechStacks = Db.Select(query).GroupBy(x => x.Id).Select(x => x.First()).ToList()
            };
        }

        private TechStackResponse GetTechnologyStackWithDetails(TechStack request)
        {
            var technologyChoices = Db.LoadSelect<TechnologyChoice>(Db.From<TechnologyChoice>()
                        .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                        .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId)
                        .Where(techChoice => techChoice.TechnologyStackId == request.Id));
            var techStack = Db.SingleById<TechnologyStack>(request.Id);

            var result = techStack.ConvertTo<TechStackDetails>();
            if (!string.IsNullOrEmpty(techStack.Details))
            {
                result.DetailsHtml = new Markdown().Transform(techStack.Details);
            }
            
            result.PopulateTechTiers(technologyChoices);

            var response = new TechStackResponse
            {
                TechStack = result
            };
            return response;
        }

    }
}
