using System.Collections.Generic;
using AuthProject.AuthService;
using AuthProject.ValueTypes;

namespace AuthProject.WorkflowTest
{
    public class CreateNewUserInputDto
    {
        public Email Email { get; set; }
        public Password Password { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public string UserName { get; set; }
    }
}