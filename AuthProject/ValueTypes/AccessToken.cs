using System;

namespace AuthProject.ValueTypes
{
    public class AccessToken : EfCoreValueType<AccessToken>
    {
        public AccessToken(string name) : base(name)
        {
            if (name.Split('.').Length != 3) throw new ArgumentException();
            // other validation
        }
    }
}