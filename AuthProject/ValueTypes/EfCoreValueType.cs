namespace AuthProject.ValueTypes
{
    public class EfCoreValueType<T> : ValueType<T>
        where T : class
    {
        public EfCoreValueType() : this(string.Empty)
        {
        }

        protected EfCoreValueType(string name) : base(name)
        {
        }
    }
}