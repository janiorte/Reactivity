﻿using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
    public class CurrentUser
    {
        public class Query : IRequest<User> { }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> UserManager;
            private readonly IJwtGenerator JwtGenerator;
            private readonly IUserAccessor UserAccessor;

            public Handler(UserManager<AppUser> userManager, IJwtGenerator jwtGenerator, IUserAccessor userAccessor)
            {
                UserManager = userManager;
                JwtGenerator = jwtGenerator;
                UserAccessor = userAccessor;
            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await UserManager.FindByNameAsync(UserAccessor.GetCurrentUsername());

                var refreshToken = JwtGenerator.GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await UserManager.UpdateAsync(user);

                return new User(user, JwtGenerator, refreshToken.Token);
            }
        }
    }
}