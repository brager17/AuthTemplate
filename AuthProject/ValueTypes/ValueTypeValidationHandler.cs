using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#nullable enable
namespace AuthProject.ValueTypes
{
    public abstract class ValueTypeValidationHandler<T>
    {
        protected class ValueTypeError
        {
            public ValueTypeError(Func<T, IEnumerable<ValidationResult>?> validationFunc)
            {
                ValidationFunc = validationFunc;
            }

            protected virtual Func<T, IEnumerable<ValidationResult>?> ValidationFunc { get; }
        }


        protected abstract IReadOnlyCollection<ValueTypeError> Validators { get; }
    }
}