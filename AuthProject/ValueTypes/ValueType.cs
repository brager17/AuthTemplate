using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

#nullable enable
namespace AuthProject.ValueTypes
{
    public class ValueType<T>
        where T : class
    {
        public string Name { get; set; }

        protected ValueType(string name)
        {
            Name = name;
        }

        public static implicit operator ValueType<T>(string value)
        {
            return (ValueType<T>) Activator.CreateInstance(typeof(T), value);
        }

        public static implicit operator string(ValueType<T> valueType)
        {
            return valueType.Name;
        }


        public static implicit operator T(ValueType<T> t)
        {
            return (T) Activator.CreateInstance(typeof(T), t.Name);
        }
    }
}