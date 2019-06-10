using System.Threading;
using System.Threading.Tasks;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.ValueTypes;
using Force;
using Microsoft.AspNetCore.Identity;

namespace AuthProject.AuthService
{
    public class PrimaryRecoveryForgotPasswordHandler : IAsyncHandler<ForgotPasswordDto, SimplyHandlerResult>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly EmailSenderService _emailSenderService;

        public PrimaryRecoveryForgotPasswordHandler(
            UserManager<CustomIdentityUser> userManager,
            EmailSenderService emailSenderService)
        {
            _userManager = userManager;
            _emailSenderService = emailSenderService;
        }

        public async Task<SimplyHandlerResult> Handle(ForgotPasswordDto input, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var sendeeEmail = (Email) input.Email;
            var subject = user.UserName;
            var text = $"Код для восстановления пароля {code}";
            var emailSenderDto = new EmailSendDto(sendeeEmail, text, subject);

            await _emailSenderService.Handle(emailSenderDto, cancellationToken);
            return new SimplyHandlerResult(true);
        }
    }
}