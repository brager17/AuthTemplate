using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AuthProject.Context;
using AuthProject.Identities;
using AuthProject.Services;
using AuthProject.ValueTypes;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.ApplicationServices;

namespace AuthProject.AuthService
{
    public class JwtAuthorizeService
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        private readonly JwtTokenOptions jwtTokenOptions;

        public JwtAuthorizeService(
            IOptions<JwtTokenOptions> JwtTokenOptions,
            UserManager<CustomIdentityUser> userManager,
            AuthDbContext authDbContext)
        {
            jwtTokenOptions = JwtTokenOptions.Value;
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public async Task<AccessRefreshDto> GetJwtToken(CustomIdentityUserDto userAuthenticationInfo,
            CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(userAuthenticationInfo.UserEmail);
            var checkPasword = await _userManager.CheckPasswordAsync(user, userAuthenticationInfo.Password);
            if (!checkPasword)
                throw new ArgumentException();

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var claims = MakeClaimsCollection(userAuthenticationInfo);


            var accessToken = (AccessToken) jwtTokenHandler.WriteToken(new JwtSecurityToken(
                claims: claims,
                issuer: jwtTokenOptions.Issuer,
                audience: jwtTokenOptions.Audience,
                expires: DateTime.Now.Add(TimeSpan.FromSeconds(30)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenOptions.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
            ));

            var refreshToken = (RefreshToken) Guid.NewGuid().ToString();


            user.RefreshToken = refreshToken;
            await _authDbContext.SaveChangesAsync(ct);

            return new AccessRefreshDto(accessToken, refreshToken);
        }

        private IEnumerable<Claim> MakeClaimsCollection(CustomIdentityUserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.NameIdentifier, user.UserName)
            };
            return claims;
        }
    }
}