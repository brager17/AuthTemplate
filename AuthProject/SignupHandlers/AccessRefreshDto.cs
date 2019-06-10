using AuthProject.ValueTypes;

namespace AuthProject.Services
{
    public class AccessRefreshDto
    {
        public AccessRefreshDto(AccessToken accessToken, RefreshToken refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public AccessToken AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}