using System;
using System.Reflection;

namespace SnowLib.DB
{
    /// <summary>
    /// Доступ к сложному (пользовательского типа) свойству элемента
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    /// <typeparam name="TValue">Свойство элемента</typeparam>
    internal class SpmComplexAccessor<TItem, TValue> :
            SpmAccessorBase<TItem, TValue> where TValue : new()
    {
        private readonly Type valueType;
        private readonly int propertyToken;

        public SpmComplexAccessor(PropertyInfo property, SpmAttribute mappedAttribute)
            : base(property, mappedAttribute)
        {
            valueType = typeof(TValue);
            this.propertyToken = property.MetadataToken;
            if (valueType.IsClass && mappedAttribute.ShareId == String.Empty)
                mappedAttribute.ShareId = property.Name;
        }

        public override void Read(ISpmContext context, int index, TItem dest)
        {
            SpmItem<TValue> itemReader = (SpmItem<TValue>)context[this.propertyToken];
            if (itemReader == null)
            {
                itemReader = new SpmItem<TValue>(context, this.Mapped, null);
                context[this.propertyToken] = itemReader;
            }
            this.setter(dest, itemReader.Read());
        }

        public override bool IsNull(ISpmContext context, int index)
        {
            SpmItem<TValue> itemReader = (SpmItem<TValue>)context[this.propertyToken];
            if (itemReader == null)
            {
                itemReader = new SpmItem<TValue>(context, this.Mapped, null);
                context[this.propertyToken] = itemReader;
            }
            return itemReader.IsKeysNull();
        }
    }
}
