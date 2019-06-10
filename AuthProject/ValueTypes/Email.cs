using System;
using System.ComponentModel.DataAnnotations;

namespace AuthProject.ValueTypes
{
    public class Email : ValueType<Email>
    {
        public Email(string name) : base(name)
        {
            var emailAddressAttribute = new EmailAddressAttribute();
            if (!emailAddressAttribute.IsValid(name))
                throw new ArgumentException(emailAddressAttribute.ErrorMessage);
        }

        public static implicit operator Email(string email)
        {
            return new Email(email);
        }
    }
}