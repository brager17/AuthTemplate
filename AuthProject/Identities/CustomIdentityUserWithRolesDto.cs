using System.Collections.Generic;
#nullable enable
namespace AuthProject.Identities
{
    public class CustomIdentityUserWithRolesDto : CustomIdentityUserDto
    {
        public IEnumerable<CustomIdentityRole>? Roles { get; set; }

        public CustomIdentityUserWithRolesDto(string userName, string userEmail, string password) : base(userName,
            userEmail, password)
        {
        }
    }
}