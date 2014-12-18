using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SnowLib
{
    /// <summary>
    /// List stored sorted by GetHashCode for fast find operation
    /// </summary>
    /// <typeparam name="T">Element type, must have correct and fast GetHashCode() method</typeparam>
    /// <remarks>Copyright (c) 2014 PSN</remarks>
    public class FindList<T> : IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IComparer<T>
    {
        #region Nested structures classes
        /// <summary>
        /// Typed and untyped enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly FindList<T> list;
            private T current;
            private int version;
            private int index;

            internal Enumerator(FindList<T> findList)
            {
                this.index = 0;
                this.current = default(T);
                this.version = findList.version;
                this.list = findList;
            }

            public T Current
            {
                get { return this.current; }
            }

            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (this.version != list.version)
                    throw new InvalidOperationException("List has been changed");
                if (this.index < list.size)
                {
                    this.current = this.list.items[this.index];
                    this.index++;
                    return true;
                }
                else
                {
                    this.current = default(T);
                    return false;
                }
            }

            public void Reset()
            {
                this.index = 0;
                this.current = default(T);
                this.version = this.list.version;
            }
        }
        #endregion

        #region Private fields
        private T[] items;
        private int size;
        private int version;
        #endregion

        #region Constructors and initializing
        public FindList()
            : this(4)
        {

        }

        public FindList(int capacity)
        {
            this.items = new T[capacity];
        }
        #endregion

        #region Operations
        private void insert(int index, T item)
        {
            if (index > this.size)
                throw new ArgumentOutOfRangeException("index");
            if (this.size == this.items.Length)
                Array.Resize<T>(ref this.items, this.items.Length * 2);
            if (index < this.size)
                Array.Copy(this.items, index, this.items, index + 1, this.size - index);
            this.items[index] = item;
            this.size++;
            this.version++;
        }

        public void Assign(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.items = collection.OrderBy(m => m.GetHashCode()).ToArray();
            this.size = this.items.Length;
            this.version = 0;
        }

        public int Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            int index = Array.BinarySearch<T>(this.items, 0, this.size, item, this);
            if (index < 0)
                index = ~index;
            else
            {
                int ihc = item.GetHashCode();
                for (int i = index; i < this.size && this.items[i].GetHashCode() == ihc; i++)
                    if (this.items[i].Equals(item))
                        return -1;
                for (int i = index - 1; i >= 0 && this.items[i].GetHashCode() == ihc; i--)
                    if (this.items[i].Equals(item))
                        return -1;
            }
            insert(index, item);
            return index;
        }

        public int Find(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            int index = Array.BinarySearch<T>(this.items, 0, this.size, item, this);
            if (index >= 0)
            {
                int ihc = item.GetHashCode();
                for (int i = index; i < this.size && this.items[i].GetHashCode() == ihc; i++)
                    if (this.items[i].Equals(item))
                        return i;
                for (int i = index - 1; i >= 0 && this.items[i].GetHashCode() == ihc; i--)
                    if (this.items[i].Equals(item))
                        return i;
            }
            return -1;
        }

        public bool Remove(T item)
        {
            int index = Find(item);
            if (index < 0)
                return false;
            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < this.size)
            {
                this.size--;
                if (index < this.size)
                    Array.Copy(this.items, index + 1, this.items, index, this.size - index);
                this.items[this.size] = default(T);
                this.version++;
            }
            else
                throw new IndexOutOfRangeException("index");
        }

        public void Clear()
        {
            for (int index = 0; index < this.size; index++)
                this.items[index] = default(T);
            this.size = 0;
        }
        #endregion

        #region Comparing T by hashcode
        int IComparer<T>.Compare(T x, T y)
        {
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
        #endregion

        #region IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T> interfaces
        public T this[int index]
        {
            get
            {
                if (index < size)
                    return items[index];
                else
                    throw new IndexOutOfRangeException("index");
            }
        }

        public int Count
        {
            get { return this.size; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion
    }
}
