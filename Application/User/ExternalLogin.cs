using Application.Errors;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
    public class ExternalLogin
    {
        public class Query : IRequest<User>
        {
            public string AccessToken { get; set; }
        }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> UserManager;
            private readonly IFacebookAccessor FacebookAccessor;
            private readonly IJwtGenerator JwtGenerator;

            public Handler(UserManager<AppUser> userManager, IFacebookAccessor facebookAccessor, IJwtGenerator jwtGenerator)
            {
                UserManager = userManager;
                FacebookAccessor = facebookAccessor;
                JwtGenerator = jwtGenerator;
            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                var userInfo = await FacebookAccessor.FacebookLogin(request.AccessToken);

                if (userInfo == null)
                    throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem validating token" });

                var user = await UserManager.FindByEmailAsync(userInfo.Email);

                var refreshToken = JwtGenerator.GenerateRefreshToken();

                if (user != null)
                {
                    user.RefreshTokens.Add(refreshToken);
                    await UserManager.UpdateAsync(user);
                    return new User(user, JwtGenerator, refreshToken.Token);
                }

                user = new AppUser
                {
                    DisplayName = userInfo.Name,
                    Id = userInfo.Id,
                    Email = userInfo.Email,
                    UserName = "fb_" + userInfo.Id
                };

                var photo = new Photo
                {
                    Id = "fb_" + userInfo.Id,
                    Url = userInfo.Picture.Data.Url,
                    IsMain = true
                };

                user.Photos.Add(photo);
                user.RefreshTokens.Add(refreshToken);

                var result = await UserManager.CreateAsync(user);

                if (!result.Succeeded)
                    throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem creating user" });

                return new User(user, JwtGenerator, refreshToken.Token);
            }
        }
    }
}