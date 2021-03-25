using Application.Interfaces;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;

namespace Application.Activities
{
    public class FollowingResolver : IValueResolver<UserActivity, AttendeeDto, bool>
    {
        private readonly DataContext Context;
        private readonly IUserAccessor UserAccesor;

        public FollowingResolver(DataContext context, IUserAccessor userAccesor)
        {
            Context = context;
            UserAccesor = userAccesor;
        }

        public bool Resolve(UserActivity source, AttendeeDto destination, bool destMember, ResolutionContext context)
        {
            var currentUser = Context.Users.SingleOrDefaultAsync(x => x.UserName == UserAccesor.GetCurrentUsername()).Result;

            if (currentUser.Followings.Any(x => x.TargetId == source.AppUserId))
                return true;

            return false;
        }
    }
}