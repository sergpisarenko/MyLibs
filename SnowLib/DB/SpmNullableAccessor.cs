using System;
using System.Reflection;
using System.Data.SqlClient;

namespace SnowLib.DB
{
    /// <summary>
    /// Реализация доступа к Nullable (?) свойствам элемента
    /// </summary>
    /// <typeparam name="TItem">Тип элемента </typeparam>
    /// <typeparam name="TValue">Чистый тип свойства (без Nullable) - 
    /// должен быть типа struct</typeparam>
    internal class SpmNullableAccessor<TItem, TValue> :
        SpmAccessorBase<TItem, Nullable<TValue>> where TValue : struct
    {
        private readonly Func<SqlDataReader, int, TValue> read;

        public SpmNullableAccessor(PropertyInfo property, SpmAttribute mappedAttribute,
            Func<SqlDataReader, int, TValue> readValue)
            : base(property, mappedAttribute)
        {
            this.read = readValue;
        }

        public override void Read(ISpmContext context, int index, TItem dest)
        {
            if (context.DataReader.IsDBNull(index))
                this.setter(dest, null);
            else
                this.setter(dest, read(context.DataReader, index));
        }
    }
}
