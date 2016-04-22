using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SnowLib.DB
{
    /// <summary>
    /// Основной класс, реализающий базовую логику чтения данных,
    /// создания новых элементов или выборки их из пула разделяемых
    /// элементов и инициализации свойств.
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    internal class SpmItem<TItem> where TItem : new()
    {
        private struct SprIndexedAccessor
        {
            public readonly int Index;
            public readonly ISpmAccessor<TItem> Accessor;

            public SprIndexedAccessor(int index, ISpmAccessor<TItem> accessor)
            {
                this.Index = index;
                this.Accessor = accessor;
            }
        }
        private readonly ISpmContext context;
        private readonly List<SprIndexedAccessor> accessors;
        private readonly List<SprIndexedAccessor> keys;
        private readonly IDictionary<TItem, TItem> sharedItems;
        private readonly bool extendable;
        private TItem sharedItem;
        private readonly SpmAttribute MappedHolder;

        public SpmItem(ISpmContext readContext, SpmAttribute mappedHolder, string group)
        {
            if (readContext == null)
                throw new ArgumentNullException("itemReader");
            this.context = readContext;
            this.MappedHolder = mappedHolder;
            ReadOnlyCollection<ISpmAccessor<TItem>> coll = SpmAccessors<TItem>.Collection;
            this.accessors = new List<SprIndexedAccessor>(coll.Count);
            this.keys = new List<SprIndexedAccessor>(4);
            for (int i = 0; i < coll.Count; i++)
            {
                ISpmAccessor<TItem> accessor = coll[i];
                if (accessor.Mapped.InGroup(group))
                {
                    string[] colNames = accessor.Mapped.Columns;
                    int index = -1;
                    for (int j = 0; j < colNames.Length; j++ )
                    {
                        string colName = colNames[j];
                        if (colName.StartsWith("/"))
                        {
                            if (this.MappedHolder == null || !this.MappedHolder.HasSynonyms)
                                throw new ArgumentException(String.Concat("Не заданы родительские синонимы для", colNames[j]));
                            if (String.IsNullOrEmpty((colName = this.MappedHolder.GetSynonim(colName.Substring(1)))))
                                throw new ArgumentException(String.Concat("Не найдена ссылка на родительский синоним ", colNames[j]));
                        }
                        if ((index = this.context.TryGetIndex(colName)) >= 0)
                            break;
                    }
                    if (index < 0 && colNames.Length > 0)
                        throw new ArgumentException(String.Concat("В результате запроса не найдено хотя бы одно из полей: ",
                            String.Join(", ", colNames)));
                    SprIndexedAccessor indexedAccessor = new SprIndexedAccessor(index, accessor);
                    this.accessors.Add(indexedAccessor);
                    if (accessor.Mapped.Key)
                        keys.Add(indexedAccessor);
                }
            }
            this.accessors.TrimExcess();
            if (mappedHolder!=null && mappedHolder.Shared)
            {
                this.sharedItems = readContext.SharedPool.Get<TItem>(mappedHolder.ShareId, out this.extendable);
                if (this.sharedItems == null)
                {
                    this.sharedItems = new Dictionary<TItem, TItem>(10);
                    readContext.SharedPool.Set(mappedHolder.ShareId, this.sharedItems, true);
                    this.extendable = true;
                }
                else
                    if (this.keys.Count == 0 && !this.extendable)
                        throw new ArgumentException("Не заданы ключи для использования фиксированного пула элементов.");
                this.sharedItem = new TItem();
            }
        }

        public TItem Read()
        {
            TItem result;
            if (IsKeysNull())
                result = default(TItem);
            else
            {
                if (this.sharedItems == null)
                {
                    result = new TItem();
                    readProperties(this.accessors, result);
                }
                else
                {
                    if (this.extendable)
                    {
                        readProperties(this.accessors, this.sharedItem);
                        if (!this.sharedItems.TryGetValue(this.sharedItem, out result))
                        {
                            result = this.sharedItem;
                            this.sharedItems.Add(result, result);
                            this.sharedItem = new TItem();
                        }
                    }
                    else
                    {
                        readProperties(this.keys, this.sharedItem);
                        this.sharedItems.TryGetValue(this.sharedItem, out result);
                    }
                }
            }
            return result;
        }

        private void readProperties(List<SprIndexedAccessor> list, TItem target)
        {
            ISpmUpdateProperties write = target as ISpmUpdateProperties;
            if (write != null)
                write.BeginUpdateProperties();
            for (int i = 0; i < list.Count; i++)
                list[i].Accessor.Read(this.context, list[i].Index, target);
            if (write != null)
                write.EndUpdateProperties();
        }

        public void Read(TItem item)
        {
            readProperties(this.accessors, item);
        }

        public bool IsKeysNull()
        {
            return this.keys.Count > 0 && this.keys.TrueForAll(isKeyNull);
        }

        private bool isKeyNull(SprIndexedAccessor entry)
        {
            return entry.Accessor.IsNull(this.context, entry.Index);
        }

    }
}
