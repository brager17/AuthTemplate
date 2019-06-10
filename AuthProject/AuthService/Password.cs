using AuthProject.ValueTypes;

namespace AuthProject.AuthService
{
    public class Password : ValueType<Password>
    {
        public Password(string name) : base(name)
        {
            // validation
        }

        public static implicit operator Password(string value)
        {
            return new Password(value);
        }
    }
}