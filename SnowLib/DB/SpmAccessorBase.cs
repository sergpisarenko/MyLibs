using System;
using System.Collections.Generic;
using System.Reflection;

namespace SnowLib.DB
{
    /// <summary>
    /// Бзаовый класс для доступа к свойствам элементов
    /// </summary>
    /// <typeparam name="TItem">Тип элемента </typeparam>
    /// <typeparam name="TValue">Тип свойства</typeparam>
    internal abstract class SpmAccessorBase<TItem, TValue> : ISpmAccessor<TItem>
    {
        protected readonly Func<TItem, TValue> getter;
        protected readonly Action<TItem, TValue> setter;
        private readonly SpmAttribute mapped;

        public SpmAccessorBase(PropertyInfo property, SpmAttribute mappedAttribute)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            if (mappedAttribute == null)
                throw new ArgumentNullException("mappedAttribute");
            MethodInfo getMethod = property.GetGetMethod(true);
            MethodInfo setMethod = property.GetSetMethod(true);
            if (getMethod == null || setMethod == null)
                throw new ArgumentException(String.Concat("Property \"", property.Name, "\" of type \"",
                    property.ReflectedType.Name, "\" marked with SqlMapped must have both get and set accessor"));
            this.getter = (Func<TItem, TValue>)getMethod.CreateDelegate(typeof(Func<TItem, TValue>));
            this.setter = (Action<TItem, TValue>)setMethod.CreateDelegate(typeof(Action<TItem, TValue>));
            this.mapped = mappedAttribute;
        }

        public SpmAttribute Mapped
        {
            get { return this.mapped; }
        }

        public abstract void Read(ISpmContext context, int index, TItem dest);

        public void Copy(TItem source, TItem dest)
        {
            setter(dest, getter(source));
        }

        public virtual bool IsEquals(TItem x, TItem y)
        {
            TValue xv = getter(x);
            TValue yv = getter(y);
            return EqualityComparer<TValue>.Default.Equals(xv, yv);
        }

        public virtual bool IsNull(ISpmContext context, int index)
        {
            return context.DataReader.IsDBNull(index);
        }
    }
}
