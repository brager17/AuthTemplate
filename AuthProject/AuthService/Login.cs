using AuthProject.ValueTypes;

namespace AuthProject.AuthService
{
    public class Login : ValueType<Login>
    {
        public Login(string name) : base(name)
        {
            // validation
        }
    }
}