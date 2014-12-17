using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib
{
    /// <summary>
    /// Standard Add/Insert/Remove collection with pre- and post- action event subscribing
    /// </summary>
    /// <typeparam name="T">Collection element type</typeparam>
    public class EventList<T> : IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IList<T>, ICollection<T>
    {
        #region Nested structures and classes
        /// <summary>
        /// Typed and untyped enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly EventList<T> list;
            private int version;
            private int index;

            internal Enumerator(EventList<T> eventList)
            {
                this.index = -1;
                this.version = eventList.version;
                this.list = eventList;
            }

            public T Current
            {
                get { return this.list.items[index]; }
            }

            object IEnumerator.Current
            {
                get { return this.list.items[index]; }
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (this.version != list.version)
                    throw new InvalidOperationException("List has been changed");
                return ++index < this.list.size;
            }

            public void Reset()
            {
                this.index = -1;
                this.version = this.list.version;
            }
        }

        /// <summary>
        /// Action event arguments
        /// </summary>
        public class ItemEventArgs : EventArgs
        {
            public readonly T Item;
            public readonly int Index;

            internal ItemEventArgs(int index, T item)
            {
                this.Index = index;
                this.Item = item;
            }
        }
        #endregion

        #region Private fields
        private T[] items;
        private int size;
        private int version;
        #endregion

        #region Public properties
        /// <summary>
        /// Total elements count
        /// </summary>
        public int Count { get { return this.size; } }
        
        /// <summary>
        /// Element at position index
        /// </summary>
        /// <param name="index">Index of the element</param>
        /// <returns>Value of the element</returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.size)
                    throw new IndexOutOfRangeException("index");
                return this.items[index];
            }
            set
            {
                if (index < 0 || index >= this.size)
                    throw new IndexOutOfRangeException("index");
                OnRemoving(index, this.items[index]);
                OnInserting(index, value);
                this.items[index] = value;
                OnChanged();
            }
        }
        /// <summary>
        /// Event before insert
        /// </summary>
        public event EventHandler<ItemEventArgs> Inserting;
        
        /// <summary>
        /// Event before remove
        /// </summary>
        public event EventHandler<ItemEventArgs> Removing;

        /// <summary>
        /// Event before clear
        /// </summary>
        public event EventHandler Clearing;
        
        /// <summary>
        /// Event after insert/remove/clear
        /// </summary>
        public event EventHandler Changed;
        
        /// <summary>
        /// Always writable
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region Constructors and initializing
        public EventList():this(4)
        {

        }

        public EventList(int capacity)
        {
            this.items = new T[capacity];
        }
        #endregion

        #region Main operations
        private void insert(int index, T item)
        {
            if (index > this.size)
                throw new ArgumentOutOfRangeException("index");
            OnInserting(index, item);
            if (this.size == this.items.Length)
                Array.Resize<T>(ref this.items, this.items.Length * 2);
            if (index < this.size)
                Array.Copy(this.items, index, this.items, index + 1, this.size - index);
            this.items[index] = item;
            this.size++;
            this.version++;
            OnChanged();
        }

        public void Add(T item)
        {
            insert(this.size, item);
        }

        public void AddRange(params T[] item)
        {
            for(int i=0; i<item.Length; i++)
                OnInserting(this.size+i, item[i]);
            if (this.size + item.Length >= this.items.Length)
                Array.Resize<T>(ref this.items, this.size + item.Length);
            Array.Copy(item, 0, this.items, this.size, item.Length);
            this.size += item.Length;
            this.version++;
            OnChanged();
        }

        public void Insert(int index, T item)
        {
            insert(index, item);
        }

        public bool Remove(T item)
        {
            int index = Array.IndexOf<T>(this.items, item, 0, this.size);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            else
                return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.size)
                throw new IndexOutOfRangeException("index");
            OnRemoving(index, this.items[index]);
            this.size--;
            if (index < this.size)
                Array.Copy(this.items, index + 1, this.items, index, this.size - index);
            this.items[this.size] = default(T);
            this.version++;
            OnChanged();
        }

        public void Clear()
        {
            OnClearing();
            for (int index = 0; index < this.size; index++)
                this.items[index] = default(T);
            this.size = 0;
            OnChanged();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(this.items, item, 0, this.size);
        }

        public bool Contains(T item)
        {
            return Array.IndexOf<T>(this.items, item, 0, this.size) >= 0;
        }

        public void CopyTo(T[] dest, int index)
        {
            Array.Copy(this.items, 0, dest, index, this.size);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < this.size; i++)
                action(this.items[i]);
        }
        #endregion

        #region Event raising methods
        protected virtual void OnRemoving(int index, T item)
        {
            ItemEventArgs args = new ItemEventArgs(index, item);
            if (this.Removing != null)
                this.Removing(this, args);
        }

        protected virtual void OnInserting(int index, T item)
        {
            ItemEventArgs args = new ItemEventArgs(index, item);
            if (this.Inserting != null)
                this.Inserting(this, args);
        }

        protected virtual void OnClearing()
        {
            if (this.Clearing != null)
                this.Clearing(this, EventArgs.Empty);
        }

        protected virtual void OnChanged()
        {
            if (this.Changed != null)
                this.Changed(this, EventArgs.Empty);
        }
        #endregion
    }
}
