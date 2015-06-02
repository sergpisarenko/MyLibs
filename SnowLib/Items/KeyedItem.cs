using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.Items
{
    /// <summary>
    /// Base class for elements with unique key
    /// </summary>
    /// <typeparam name="T">Key type</typeparam>
    public class KeyedItem<T> : IEquatable<KeyedItem<T>>, IEqualityComparer<KeyedItem<T>>
    {
        #region Public fields
        public readonly T Key;
        #endregion

        #region Constructor and initializers
        public KeyedItem(T key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            this.Key = key;
        }
        #endregion

        #region Base methods overrides
        /// <summary>
        /// Return hash code from Key because it is unique
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        /// <summary>
        /// Equality test
        /// </summary>
        /// <returns>Returns true if obj is KeyedItem and Key are equals</returns>
        public override bool Equals(object obj)
        {
            KeyedItem<T> keyedItem = obj as KeyedItem<T>;
            if (keyedItem == null)
                return false;
            return this.Key.Equals(keyedItem.Key);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>Default string representation of IdItem</returns>
        public override string ToString()
        {
            return this.Key.ToString();
        }
        #endregion

        #region IEqualityComparer<IdItem<T>>
        public bool Equals(KeyedItem<T> x, KeyedItem<T> y)
        {
            if (x == null)
                return y == null;
            else
                return y == null ? false : x.Key.Equals(y.Key);
        }

        public int GetHashCode(KeyedItem<T> obj)
        {
            return obj == null ? 0 : obj.Key.GetHashCode();
        }
        #endregion

        #region IEquatable<IdItem<T>>
        public bool Equals(KeyedItem<T> other)
        {
            return other == null ? false : this.Key.Equals(other.Key);
        }
        #endregion
    }
}
