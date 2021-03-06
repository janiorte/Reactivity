﻿using Application.Errors;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Application.User
{
    public class Login
    {
        public class Query : IRequest<User>
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> UserManager;
            private readonly SignInManager<AppUser> SignInManager;
            private readonly IJwtGenerator JwtGenerator;

            public Handler(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtGenerator jwtGenerator)
            {
                SignInManager = signInManager;
                UserManager = userManager;
                JwtGenerator = jwtGenerator;
            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await UserManager.FindByEmailAsync(request.Email);

                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized);

                if (!user.EmailConfirmed)
                    throw new RestException(HttpStatusCode.BadRequest, new { Email = "Email is not confirmed" });

                var result = await SignInManager.CheckPasswordSignInAsync(user, request.Password, false);

                if(result.Succeeded)
                {
                    var refreshToken = JwtGenerator.GenerateRefreshToken();
                    user.RefreshTokens.Add(refreshToken);
                    await UserManager.UpdateAsync(user);
                    return new User(user, JwtGenerator, refreshToken.Token);
                }

                throw new RestException(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}