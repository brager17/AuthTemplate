using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthProject.ValueTypes;
using Microsoft.AspNetCore.Identity;

namespace AuthProject.Identities
{
    public sealed class CustomIdentityUser : IdentityUser
    {
        public CustomIdentityUser()
        {
            PasswordHashEntity = string.Empty;
        }

        public CustomIdentityUser(string email, string username, string passwordHash, int age)
        {
            Email = email;
            UserName = username;
            PasswordHash = passwordHash;
            PasswordHashEntity = new HashedPassword(PasswordHash);
            Age = age;
        }

        [NotMapped]
        public string PasswordHashEntity { get; set; }

        public int Age { get; set; }

        public RefreshToken RefreshToken { get; set; } = new RefreshToken("123");
    }
}