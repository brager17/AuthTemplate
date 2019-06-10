using System;
using System.Collections.Generic;
using AuthProject.AuthService;
using AuthProject.Identities;
using AuthProject.ValueTypes;
using Force;

#nullable enable
namespace AuthProject.WorkflowTest
{
    public class Error
    {
    }

    public class InputDto
    {
        public Email Email { get; set; }
        public Password Password { get; set; }
    }

    public class CreateUserDto
    {
        public CustomIdentityUser Result { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }

    public class AddRolesDto
    {
        public string UserEmail { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }

    public class EmailSenderDto
    {
        public string UserToken { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }

    public class TestWorkflow
    {
        public class UserCreateHandler : IHandler<InputDto, CreateUserDto>
        {
            public CreateUserDto Handle(InputDto input)
            {
                throw new NotImplementedException();
            }
        }

        public class AddRolesHandler : IHandler<CreateUserDto, AddRolesDto>
        {
            public AddRolesDto Handle(CreateUserDto input)
            {
                throw new NotImplementedException();
            }
        }

        public class EmailSenderHandler:IHandler<AddRolesDto,EmailSenderDto>
        {
            public EmailSenderDto Handle(AddRolesDto input)
            {
                throw new NotImplementedException();
            }
        }
    }
}