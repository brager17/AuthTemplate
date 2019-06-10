using System.Threading;
using System.Threading.Tasks;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using Force;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthProject.Services
{
    public class UserSignupHandler : IAsyncHandler<TokenEmailDto, SimplyHandlerResult>
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<CustomIdentityUser> _userManager;

        public UserSignupHandler(
            AuthDbContext authDbContext,
            UserManager<CustomIdentityUser> userManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public async Task<SimplyHandlerResult> Handle(TokenEmailDto input, CancellationToken cancellationToken)
        {
            var user = await _authDbContext.Users.FirstOrDefaultAsync(x => x.Email == input.Email, cancellationToken);
            var s = await _userManager.ConfirmEmailAsync(user, input.Code);
            return new SimplyHandlerResult(true);
        }
    }
}