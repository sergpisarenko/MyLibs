using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SnowLib.DB
{
    /// <summary>
    /// Коллекция объектов для доступа ко все размеченным свойствам элемента
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    internal sealed class SpmAccessors<TItem> where TItem : new()
    {
        private static SpmAccessors<TItem> instance;

        public static ReadOnlyCollection<ISpmAccessor<TItem>> Collection
        {
            get
            {
                if (instance == null)
                    instance = new SpmAccessors<TItem>();
                return instance.collection;
            }
        }

        private readonly ReadOnlyCollection<ISpmAccessor<TItem>> collection;

        private SpmAccessors()
        {
            Type itemType = typeof(TItem);
            PropertyInfo[] properties = itemType.GetProperties(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            List<ISpmAccessor<TItem>> list = new List<ISpmAccessor<TItem>>(properties.Length);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                SpmAttribute mappedAttr = property.GetCustomAttribute<SpmAttribute>();
                if (mappedAttr != null)
                {
                    bool isNullable = false;
                    Type valueType = property.PropertyType;
                    if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        // nullable type
                        valueType = Nullable.GetUnderlyingType(valueType);
                        isNullable = true;

                    }
                    if (valueType.IsEnum)
                        valueType = Enum.GetUnderlyingType(valueType);
                    Delegate readDelegate = SpmReaderDelegates.Get(valueType);
                    ISpmAccessor<TItem> accessor;
                    if (readDelegate == null)
                    {
                        // user type
                        // test for default constructor
                        if (valueType.GetConstructor(Type.EmptyTypes) == null)
                            throw new ArgumentException(String.Concat("Type \"", valueType.Name, " of property \"",
                                property.Name, "\" marked with SqlMapped must have default constructor"));
                        Type propertyAccessorType =
                            typeof(SpmComplexAccessor<,>).MakeGenericType(itemType, valueType);
                        accessor = (ISpmAccessor<TItem>)
                            Activator.CreateInstance(propertyAccessorType, property, mappedAttr);
                    }
                    else
                    {
                        // standard type
                        Type propertyAccessorType = isNullable ?
                            typeof(SpmNullableAccessor<,>).MakeGenericType(itemType, valueType) :
                            typeof(SpmAccessor<,>).MakeGenericType(itemType, valueType);
                        accessor = (ISpmAccessor<TItem>)
                            Activator.CreateInstance(propertyAccessorType, property, mappedAttr, readDelegate);
                    }
                    list.Add(accessor);
                }
            }
            list.TrimExcess();
            this.collection = new ReadOnlyCollection<ISpmAccessor<TItem>>(list);
        }
    }
}
