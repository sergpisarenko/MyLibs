using System;
using System.Reflection;
using System.Data.SqlClient;

namespace SnowLib.DB
{
    /// <summary>
    /// Доступ к простому свойству элемента (int, DateTime, string...)
    /// </summary>
    /// <typeparam name="TItem">Тип элемента </typeparam>
    /// <typeparam name="TValue">Тип свойства</typeparam>
    internal class SpmAccessor<TItem, TValue> :
        SpmAccessorBase<TItem, TValue>
    {
        private readonly Func<SqlDataReader, int, TValue> read;

        public SpmAccessor(PropertyInfo property, SpmAttribute mappedAttribute,
            Func<SqlDataReader, int, TValue> readValue)
            : base(property, mappedAttribute)
        {
            this.read = readValue;
            if (mappedAttribute.Columns.Length == 0)
                mappedAttribute.ColumnNames = property.Name;
        }

        public override void Read(ISpmContext context, int index, TItem dest)
        {
            this.setter(dest, context.DataReader.IsDBNull(index) ?
                default(TValue) : read(context.DataReader, index));
        }
    }
}
