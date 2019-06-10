using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AuthProject.AuthService;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.ValueTypes;
using Force;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit.Text;

#nullable enable
namespace AuthProject.Services
{
    public class EmailConfirmationHandler : IAsyncHandler<CustomIdentityUserWithRolesDto, ConfirmationCodeDto>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly IAsyncHandler<EmailSendDto, SimplyHandlerResult> _emailSender;

        public EmailConfirmationHandler(
            UserManager<CustomIdentityUser> userManager,
            EmailSenderService emailSenderService)
        {
            _userManager = userManager;
            _emailSender = emailSenderService;
        }

        public async Task<ConfirmationCodeDto> Handle(CustomIdentityUserWithRolesDto input,
            CancellationToken cancellationToken)
        {
            var user = new CustomIdentityUser();
            try
            {
                user = new CustomIdentityUser(input.UserEmail, input.UserName, input.Password);
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    return new ConfirmationCodeDto(createUserResult.Errors);
                }

                if (input.Roles != null)
                {
                    var addRolesResult = await _userManager.AddToRolesAsync(user, input.Roles.Select(x => x.Name));
                    if (!addRolesResult.Succeeded)
                    {
                        return new ConfirmationCodeDto(addRolesResult.Errors);
                    }
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var email = (Email) input.UserEmail;
                var text = GenerateMessageText(token, email);
                var subject = input.UserName;
                var emailSendDto = new EmailSendDto(email, text, subject);

                var sendEmailResult = await _emailSender.Handle(emailSendDto, cancellationToken);

                if (!sendEmailResult.Succeeded)
                {
                }

                return new ConfirmationCodeDto(text);
            }
            catch (Exception e)
            {
                await _userManager.DeleteAsync(user);
                Console.WriteLine(e);
                throw;
            }
        }

        private string GenerateMessageText(string code, string email)
        {
            var a =
                $"<a href='http://localhost:5000/api/auth/secondaryAuthentication?code={HttpUtility.UrlEncode(code)}&email={email}'>Ссылка</a>";
            var message = $"Для успешной аутентификации перейдите по ссылке  {a}";
            return message;
        }
    }
}