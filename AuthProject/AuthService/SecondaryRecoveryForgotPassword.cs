using System.Threading;
using System.Threading.Tasks;
using AuthProject.Identities;
using AuthProject.Services;
using Force;
using Microsoft.AspNetCore.Identity;

namespace AuthProject.AuthService
{
    public class SecondaryRecoveryForgotPassword : IAsyncHandler<TokenEmailPasswordDto, ResetPasswordDto>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;

        public SecondaryRecoveryForgotPassword(UserManager<CustomIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResetPasswordDto> Handle(TokenEmailPasswordDto input, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            var verifyToken = await _userManager.ResetPasswordAsync(user, input.Code, input.NewPassword);
            return !verifyToken.Succeeded ? new ResetPasswordDto(verifyToken.Errors) : new ResetPasswordDto();
        }
    }
}