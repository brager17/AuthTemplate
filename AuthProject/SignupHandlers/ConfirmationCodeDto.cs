using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
#nullable enable
namespace AuthProject.Services
{
    public class ConfirmationCodeDto
    {
        public ConfirmationCodeDto(string code)
        {
            Code = code;
        }

        public ConfirmationCodeDto(IEnumerable<IdentityError> identityErrors)
        {
            IdentityErrors = identityErrors;
        }

        public string? Code { get; set; }
        public IEnumerable<IdentityError>? IdentityErrors { get; set; }
    }
}