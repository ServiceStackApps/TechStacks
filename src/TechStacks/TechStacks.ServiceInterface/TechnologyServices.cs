﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (!session.HasRole(RoleNames.Admin))
            {
                request.LogoApproved = false;
            }
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
            {
                throw HttpError.NotFound("Tech not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (existingTech.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
            {
                throw HttpError.Unauthorized("You are not the owner of this technology.");
            }
            if (request.LogoUrl != existingTech.LogoUrl && !session.HasRole(RoleNames.Admin))
            {
                request.LogoApproved = false;
            }
            var updated = request.ConvertTo<Technology>();
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
            {
                throw HttpError.NotFound("Tech not found");
            }
            var session = SessionAs<AuthUserSession>();
            if (existingTech.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
            {
                throw HttpError.Unauthorized("You are not the owner of this technology.");
            }
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
            {
                HttpError.NotFound("Tech stack not found");
            }
            return new TechResponse
            {
                Tech = Db.SingleById<Technology>(request.Id)
            };
        }
    }
}