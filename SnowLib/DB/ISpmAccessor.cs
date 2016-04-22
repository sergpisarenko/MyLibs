using System;

namespace SnowLib.DB
{
    /// <summary>
    /// Интерфейс свойств и операции с конкрентным свойством элемента
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    internal interface ISpmAccessor<TItem>
    {
        SpmAttribute Mapped { get; }
        void Read(ISpmContext context, int index, TItem dest);
        void Copy(TItem source, TItem dest);
        bool IsEquals(TItem x, TItem y);
        bool IsNull(ISpmContext context, int index);
    }
}
