using Application.Errors;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Followers
{
    public class Add
    {
        public class Command : IRequest
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext Context;
            private readonly IUserAccessor UserAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                Context = context;
                UserAccessor = userAccessor;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var observer = await Context.Users.SingleOrDefaultAsync(x => x.UserName == UserAccessor.GetCurrentUsername());
                var target = await Context.Users.SingleOrDefaultAsync(x => x.UserName == request.Username);

                if (target == null)
                    throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });

                var following = await Context.Followings.SingleOrDefaultAsync(x =>
                    x.ObserverId == observer.Id && x.TargetId == target.Id);

                if (following != null)
                {
                    throw new RestException(HttpStatusCode.BadRequest, new { User = "You are already following this user" });
                }

                following = new UserFollowing
                {
                    Observer = observer,
                    Target = target
                };

                Context.Followings.Add(following);

                var success = await Context.SaveChangesAsync() > 0;

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}