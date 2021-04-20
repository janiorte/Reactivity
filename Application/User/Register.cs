using Application.Errors;
using Application.Interfaces;
using Application.Validators;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
    public class Register
    {
        public class Command : IRequest
        {
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Origin { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.DisplayName).NotEmpty();
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).Password();
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext Context;
            private readonly UserManager<AppUser> UserManager;
            private readonly IEmailSender EmailSender;

            public Handler(DataContext context, UserManager<AppUser> userManager,
                IEmailSender emailSender)
            {
                Context = context;
                UserManager = userManager;
                EmailSender = emailSender;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                if (await Context.Users.AnyAsync(x => x.Email == request.Email))
                    throw new RestException(System.Net.HttpStatusCode.BadRequest, new { Email = "Email already exists" });

                if (await Context.Users.AnyAsync(x => x.UserName == request.Username))
                    throw new RestException(System.Net.HttpStatusCode.BadRequest, new { Username = "Username already exists" });

                var user = new AppUser
                {
                    DisplayName = request.DisplayName,
                    Email = request.Email,
                    UserName = request.Username
                };

                var result = await UserManager.CreateAsync(user, request.Password);

                if (!result.Succeeded) throw new Exception("Problem creating user");

                var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var verifyUrl = $"{request.Origin}/user/verifyEmail?token={token}&email={request.Email}";

                var message = $"<p>Please click the below link to verify your email address:</p>" +
                    $"<p><a href='{verifyUrl}'>{verifyUrl}</a></p>";

                await EmailSender.SendEmailAsync(request.Email, "Please verify email address", message);

                return Unit.Value;
            }
        }
    }
}