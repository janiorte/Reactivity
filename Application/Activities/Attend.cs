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

namespace Application.Activities
{
    public class Attend
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
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
                var activity = await Context.Activities.FindAsync(request.Id);

                if (activity == null)
                    throw new RestException(HttpStatusCode.NotFound,
                        new { Activity = "Could not find activity" });

                var user = await Context.Users.SingleOrDefaultAsync(x =>
                    x.UserName == UserAccessor.GetCurrentUsername());

                var attendance = await Context.UserActivities.SingleOrDefaultAsync(x =>
                    x.ActivityId == activity.Id && x.AppUserId == user.Id);

                if (attendance != null)
                    throw new RestException(HttpStatusCode.BadRequest,
                        new { Attendance = "Already attending this activity" });

                attendance = new UserActivity
                {
                    Activity = activity,
                    AppUser = user,
                    IsHost = false,
                    DateJoined = DateTime.Now
                };

                Context.UserActivities.Add(attendance);

                var success = await Context.SaveChangesAsync() > 0;

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}