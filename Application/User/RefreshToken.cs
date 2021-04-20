using Application.Errors;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
    public class RefreshToken
    {
        public class Command : IRequest<User>
        {
            public string RefreshToken { get; set; }
        }

        public class Handler : IRequestHandler<Command, User>
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

            public async Task<User> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await UserManager.FindByNameAsync(UserAccessor.GetCurrentUsername());

                var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == request.RefreshToken);

                if (oldToken != null && !oldToken.IsActive) throw new RestException(HttpStatusCode.Unauthorized);

                if (oldToken != null)
                {
                    oldToken.Revoked = DateTime.UtcNow;
                }

                var newRefreshToken = JwtGenerator.GenerateRefreshToken();
                user.RefreshTokens.Add(newRefreshToken);
                await UserManager.UpdateAsync(user);
                return new User(user, JwtGenerator, newRefreshToken.Token);
            }
        }
    }
}