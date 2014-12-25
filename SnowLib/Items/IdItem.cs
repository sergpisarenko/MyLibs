using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.Items
{
    /// <summary>
    /// Base class for elements with unique identifier (ID)
    /// Useful for database catalogs etc
    /// </summary>
    /// <typeparam name="T">ID type</typeparam>
    public class IdItem<T> : IEquatable<IdItem<T>>, IEqualityComparer<IdItem<T>>
    {
        #region Public fields
        public readonly T ID;
        #endregion

        #region Constructor and initializers
        public IdItem(T id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            this.ID = id;
        }
        #endregion

        #region Base methods overrides
        /// <summary>
        /// Return hash code from ID because it is unique
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        /// <summary>
        /// Equality test
        /// </summary>
        /// <returns>Returns true if obj is IdItem and ID are equals</returns>
        public override bool Equals(object obj)
        {
            IdItem<T> idItem = obj as IdItem<T>;
            if (idItem == null)
                return false;
            return this.ID.Equals(idItem.ID);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>Default string representation of IdItem</returns>
        public override string ToString()
        {
            return this.ID.ToString();
        }
        #endregion

        #region IEqualityComparer<IdItem<T>>
        public bool Equals(IdItem<T> x, IdItem<T> y)
        {
            if (x == null)
                return y == null;
            else
                return y == null ? false : x.ID.Equals(y.ID);
        }

        public int GetHashCode(IdItem<T> obj)
        {
            return obj == null ? 0 : obj.ID.GetHashCode();
        }
        #endregion

        #region IEquatable<IdItem<T>>
        public bool Equals(IdItem<T> other)
        {
            return other == null ? false : this.ID.Equals(other.ID);
        }
        #endregion
    }
}
