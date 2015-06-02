using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SnowLib.DB
{
    public static class SqlDataReaderExtension
    {
        private static Dictionary<Type, object> typeFieldMap = new Dictionary<Type, object>()
        {
            { typeof(bool), typeof(SqlDataReader).GetMethod("GetBoolean").CreateDelegate(typeof(Func<SqlDataReader, int, bool>)) },
            { typeof(byte), typeof(SqlDataReader).GetMethod("GetByte").CreateDelegate(typeof(Func<SqlDataReader, int, byte>)) },
            { typeof(byte[]), typeof(SqlDataReaderExtension).GetMethod("GetBytes").CreateDelegate(typeof(Func<SqlDataReader, int, byte[]>)) },
            { typeof(char[]), typeof(SqlDataReaderExtension).GetMethod("GetChars").CreateDelegate(typeof(Func<SqlDataReader, int, char[]>)) },
            { typeof(DateTime), typeof(SqlDataReader).GetMethod("GetDateTime").CreateDelegate(typeof(Func<SqlDataReader, int, DateTime>)) },
            { typeof(DateTimeOffset), typeof(SqlDataReader).GetMethod("GetDateTimeOffset").CreateDelegate(typeof(Func<SqlDataReader, int, DateTimeOffset>)) },
            { typeof(decimal), typeof(SqlDataReader).GetMethod("GetDecimal").CreateDelegate(typeof(Func<SqlDataReader, int, decimal>)) },
            { typeof(double), typeof(SqlDataReader).GetMethod("GetDouble").CreateDelegate(typeof(Func<SqlDataReader, int, double>)) },
            { typeof(float), typeof(SqlDataReader).GetMethod("GetFloat").CreateDelegate(typeof(Func<SqlDataReader, int, float>)) },
            { typeof(Guid), typeof(SqlDataReader).GetMethod("GetGuid").CreateDelegate(typeof(Func<SqlDataReader, int, Guid>)) },
            { typeof(string), typeof(SqlDataReader).GetMethod("GetString").CreateDelegate(typeof(Func<SqlDataReader, int, string>)) },
            { typeof(short), typeof(SqlDataReader).GetMethod("GetInt16").CreateDelegate(typeof(Func<SqlDataReader, int, short>)) },
            { typeof(int), typeof(SqlDataReader).GetMethod("GetInt32").CreateDelegate(typeof(Func<SqlDataReader, int, int>)) },
            { typeof(long), typeof(SqlDataReader).GetMethod("GetInt64").CreateDelegate(typeof(Func<SqlDataReader, int, long>)) }
        };

        public static byte[] GetBytes(this SqlDataReader reader, int index)
        {
            return reader.GetSqlBinary(index).Value;
        }

        public static char[] GetChars(this SqlDataReader reader, int index)
        {
            return reader.GetSqlChars(index).Value;
        }

        private interface IPropertyWriter<TItem>
        {
            void Write(SqlDataReader reader, int index, TItem dest);
        }

        private class PropertyWriter<TItem, TValue> : IPropertyWriter<TItem>
        {
            protected readonly Action<TItem, TValue> setValue;
            protected readonly Func<SqlDataReader, int, TValue> getValue;


            public PropertyWriter(PropertyInfo propertyInfo)
            {
                MethodInfo setMethod = propertyInfo.GetSetMethod(true);
                if (setMethod == null)
                    throw new ArgumentException("Property with " + propertyInfo.Name + " must have set accessors!");
                this.setValue = (Action<TItem, TValue>)setMethod.CreateDelegate(typeof(Action<TItem, TValue>));
                object getValueDelegate;
                Type typeValue = typeof(TValue);
                if (typeFieldMap.TryGetValue(typeValue, out getValueDelegate))
                    this.getValue = (Func<SqlDataReader, int, TValue>)getValueDelegate;
                else
                    throw new ArgumentException(propertyInfo.Name + ": " + propertyInfo.PropertyType.FullName + " - is illegal type.");
            }

            public void Write(SqlDataReader reader, int index, TItem dest)
            {
                this.setValue(dest, reader.IsDBNull(index) ? default(TValue) : getValue(reader, index));
            }
        }

        private class PropertyWriterNullable<TItem, TValue> : IPropertyWriter<TItem> where TValue : struct
        {
            protected readonly Action<TItem, Nullable<TValue>> setValue;
            protected readonly Func<SqlDataReader, int, TValue> getValue;

            public PropertyWriterNullable(PropertyInfo propertyInfo)
            {
                MethodInfo setMethod = propertyInfo.GetSetMethod();
                if (setMethod == null)
                    throw new ArgumentException("Property with " + propertyInfo.Name + " must have set accessors!");
                this.setValue = (Action<TItem, Nullable<TValue>>)setMethod.CreateDelegate(typeof(Action<TItem, Nullable<TValue>>));
                object getValueDelegate;
                Type typeValue = typeof(TValue);
                if (typeFieldMap.TryGetValue(typeValue, out getValueDelegate))
                    this.getValue = (Func<SqlDataReader, int, TValue>)getValueDelegate;
                else
                    throw new ArgumentException(propertyInfo.Name + ": " + propertyInfo.PropertyType.FullName + " - is illegal type.");
            }

            public void Write(SqlDataReader reader, int index, TItem dest)
            {
                if (reader.IsDBNull(index))
                    setValue(dest, null);
                else
                    setValue(dest, getValue(reader, index));
            }
        }

        private class ItemWriter<TItem>
        {
            private readonly Dictionary<string, IPropertyWriter<TItem>> propertyWriterMap;

            public ItemWriter()
            {
                Type typeItem = typeof(TItem);
                PropertyInfo[] properties = typeItem.GetProperties();
                propertyWriterMap = new Dictionary<string, IPropertyWriter<TItem>>(properties.Length);
                foreach (PropertyInfo pi in properties)
                {
                    QueryFieldAttribute attrField = pi.GetCustomAttribute<QueryFieldAttribute>();
                    if (attrField != null)
                    {
                        Type typeValue = pi.PropertyType.IsEnum ? Enum.GetUnderlyingType(pi.PropertyType) : pi.PropertyType;
                        Type typePropertyWriter = typeValue.IsGenericType && typeValue.GetGenericTypeDefinition() == typeof(Nullable<>) ?
                            typeof(PropertyWriterNullable<,>).MakeGenericType(typeItem, typeValue.GetGenericArguments()[0]) :
                            typeof(PropertyWriter<,>).MakeGenericType(typeItem, typeValue);
                        IPropertyWriter<TItem> propertyWriter = (IPropertyWriter<TItem>)Activator.CreateInstance(typePropertyWriter, pi);
                        propertyWriterMap.Add(attrField.Name, propertyWriter);
                    }
                }
            }

            public List<KeyValuePair<int, IPropertyWriter<TItem>>> GetWriterMap(SqlDataReader reader)
            {
                List<KeyValuePair<int, IPropertyWriter<TItem>>> list =
                    new List<KeyValuePair<int, IPropertyWriter<TItem>>>(propertyWriterMap.Count);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    IPropertyWriter<TItem> propertyWriter;
                    if (propertyWriterMap.TryGetValue(reader.GetName(i), out propertyWriter))
                        list.Add(new KeyValuePair<int, IPropertyWriter<TItem>>(i, propertyWriter));
                }
                return list;
            }

            public void Write(SqlDataReader reader, TItem dest)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    IPropertyWriter<TItem> propertyWriter;
                    if (propertyWriterMap.TryGetValue(reader.GetName(i), out propertyWriter))
                        propertyWriter.Write(reader, i, dest);
                }
            }
        }

        private static Dictionary<Type, object> itemWriterMap = new Dictionary<Type, object>(100);

        private static ItemWriter<T> getWriter<T>()
        {
            Type typeItem = typeof(T);
            ItemWriter<T> itemWriter;
            object itemWriterHolder;
            if (!itemWriterMap.TryGetValue(typeItem, out itemWriterHolder))
            {
                itemWriter = new ItemWriter<T>();
                itemWriterMap.Add(typeItem, itemWriter);
            }
            else
                itemWriter = (ItemWriter<T>)itemWriterHolder;
            return itemWriter;
        }

        public static T GetAs<T>(this SqlDataReader reader) where T : new()
        {
            ItemWriter<T> itemWriter = getWriter<T>();
            T item = new T();
            itemWriter.Write(reader, item);
            return item;
        }

        public static IEnumerable<T> YieldAs<T>(this SqlDataReader reader) where T : new()
        {
            ItemWriter<T> itemWriter = getWriter<T>();
            List<KeyValuePair<int, IPropertyWriter<T>>> indexMap = itemWriter.GetWriterMap(reader);
            while (reader.Read())
            {
                T item = new T();
                indexMap.ForEach(m => m.Value.Write(reader, m.Key, item));
                yield return item;
            }
        }

    }
}
