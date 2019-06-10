using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
#nullable enable
namespace AuthProject.Identities
{
    public class ResetPasswordDto
    {
        public ResetPasswordDto(IEnumerable<IdentityError> identityErrors)
        {
            IdentityErrors = identityErrors;
        }

        public ResetPasswordDto()
        {
            IsSuccess = true;
        }

        public IEnumerable<IdentityError>? IdentityErrors { get; set; }
        public bool IsSuccess { get; set; }
    }
}