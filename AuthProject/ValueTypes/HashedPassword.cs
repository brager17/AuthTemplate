using System;

namespace AuthProject.ValueTypes
{
    public class HashedPassword : ValueType<HashedPassword>
    {
        public string EncodePassword()
        {
            // todo реализовать расхэширование паролей
            throw new NotImplementedException();
        }

        public HashedPassword(string name) : base(name)
        {
        }
    }
}