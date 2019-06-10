using AuthProject.Identities;
using AuthProject.ValueTypes;
#nullable enable
namespace AuthProject.Context
{
    public class DisabledAccessTokenValueObject
    {
        public int Id { get; set; }
        public CustomIdentityUser? CustomIdentityUser;

        protected DisabledAccessTokenValueObject()
        {
        }

        public DisabledAccessTokenValueObject(CustomIdentityUser? customIdentityUser, AccessToken? accessToken)
        {
            CustomIdentityUser = customIdentityUser;
            AccessToken = accessToken;
        }

        public AccessToken? AccessToken { get; set; }
    }
}