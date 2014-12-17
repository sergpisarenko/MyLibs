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
    /// Element list stored sorted by GetHashCode for fast serach
    /// </summary>
    /// <typeparam name="T">Element type, must have correct GetHashCode() method</typeparam>
    public class FindList<T> : IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IComparer<KeyValuePair<int, T>>
    {
        #region Nested structures and classes
        /// <summary>
        /// Typed and untyped enumerators
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
                    this.current = this.list.items[this.index].Value;
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
        private KeyValuePair<int, T>[] items;
        private int size;
        private int version;
        #endregion

        #region Constructors and initializing
        public FindList():this(4)
        {

        }

        public FindList(int capacity)
        {
            this.items = new KeyValuePair<int,T>[capacity];
        }
        #endregion

        #region Main operations
        private void insert(int index, KeyValuePair<int, T> item)
        {
            if (index > this.size)
                throw new ArgumentOutOfRangeException("index");
            if (this.size == this.items.Length)
                Array.Resize<KeyValuePair<int,T>>(ref this.items, this.items.Length * 2);
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
            this.items = collection.Select(m => new KeyValuePair<int, T>(m.GetHashCode(), m)).OrderBy(m => m.Key).ToArray();
            this.size = this.items.Length;
            this.version = 0;
        }

        public int Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            KeyValuePair<int, T> innerItem = new KeyValuePair<int, T>(item.GetHashCode(), item);
            int index = Array.BinarySearch<KeyValuePair<int, T>>(this.items, 0, this.size, innerItem, this);
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
            insert(index, innerItem);
            return index;
        }

        public int Find(T item)
        {
            if (item==null)
                throw new ArgumentNullException("item");
            KeyValuePair<int, T> innerItem = new KeyValuePair<int, T>(item.GetHashCode(), item);
            int index = Array.BinarySearch<KeyValuePair<int, T>>(this.items, 0, this.size, innerItem, this);
            if (index>=0)
            {
                int ihc = item.GetHashCode();
                for(int i=index; i<this.size && this.items[i].GetHashCode()==ihc; i++)
                    if (this.items[i].Equals(item))
                        return i;
                for(int i=index-1; i>=0 && this.items[i].GetHashCode()==ihc; i--)
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
                this.items[this.size] = default(KeyValuePair<int, T>);
                this.version++;
            }
            else
                throw new IndexOutOfRangeException("index");
        }
        #endregion

        #region Comparing IComparer<T>
        int IComparer<KeyValuePair<int, T>>.Compare(KeyValuePair<int, T> x, KeyValuePair<int, T> y)
        {
            return x.Key.CompareTo(y.Key);
        }
        #endregion

        #region IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T> interfaces
        public T this[int index]
        {
            get 
            {
                if (index < size)
                    return items[index].Value;
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
