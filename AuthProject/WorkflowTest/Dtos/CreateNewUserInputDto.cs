using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AuthProject.AuthService;
using AuthProject.ValueTypes;

namespace AuthProject.WorkflowTest
{
    public class CreateNewUserInputDto
    {
        [Required]
        public Email Email { get; set; }
        [Required]
        public Password Password { get; set; }
        [Required]
        public IEnumerable<string> Roles { get; set; }
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public int Age { get; set; }
    }
}