using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
    public class ResendEmailVerification
    {
        public class Query : IRequest
        {
            public string Email { get; set; }
            public string Origin { get; set; }
        }

        public class Handler : IRequestHandler<Query>
        {
            private readonly UserManager<AppUser> UserManager;
            private readonly IEmailSender EmailSender;

            public Handler(UserManager<AppUser> user, IEmailSender emailSender)
            {
                UserManager = user;
                EmailSender = emailSender;
            }

            public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await UserManager.FindByEmailAsync(request.Email);

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