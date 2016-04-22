using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SnowLib.DB
{
    /// <summary>
    /// Пул разделяемых ссылочных элементов
    /// </summary>
    public class SpmSharedItemPool
    {
        private struct SpmItemPoolEntry
        {
            public readonly string Id;
            public readonly object Items;
            public readonly bool Extendable;
            
            public SpmItemPoolEntry(string id, object items, bool extendable)
            {
                this.Id = id;
                this.Items = items;
                this.Extendable = extendable;
            }
        }

        private readonly List<SpmItemPoolEntry> pool;

        public SpmSharedItemPool()
        {
            this.pool = new List<SpmItemPoolEntry>();
        }

        public void Set<TItem>(string id, IDictionary<TItem, TItem> items, bool extendable = false)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (items == null)
                throw new ArgumentNullException("items");
            int index = this.pool.FindIndex(m => m.Id == id);
            SpmItemPoolEntry entry = new SpmItemPoolEntry(id, items, extendable);
            if (index < 0)
                this.pool.Add(entry);
            else
                this.pool[index] = entry;
        }

        public void Remove(string id)
        {
            int index = this.pool.FindIndex(m => m.Id == id);
            if (index>=0)
                this.pool.RemoveAt(index);
        }

        public IDictionary<TItem, TItem> Get<TItem>(string id, out bool extendable)
        {
            int index = this.pool.FindIndex(m => m.Id == id);
            if (index < 0)
            {
                extendable = false;
                return null;
            }
            else
            {
                extendable = this.pool[index].Extendable;
                return this.pool[index].Items as IDictionary<TItem, TItem>;
            }
        }
    }
}
