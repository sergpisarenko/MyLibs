using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.Items
{
    /// <summary>
    /// Элемент с уникальными идентификатором
    /// </summary>
    /// <typeparam name="TId">Тип идентификатора</typeparam>
    /// <remarks>Тип идентификатора должен корректно реализовывать 
    /// операции Equals, GetHashCode</remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public class IdentityItem<TId> : IEquatable<IdentityItem<TId>>
    {
        // Идентификатор элемента
        [DataMember]
        protected TId id;

        // 
        /// <summary>
        /// Является ли текущее значение идентификатора значением по умолчанию 
        /// </summary>
        public bool IsIdUspecified
        {
            get 
            {
                IEquatable<TId> eqt = id as IEquatable<TId>;
                if (eqt == null)
                {
                    return this.id == null ? 
                        true : Object.Equals(this.id, default(TId));
                }
                else
                    return eqt.Equals(default(TId));
            }
        }

        /// <summary>
        /// Сравнивает текущий и заданный элементы согласно их идентификаторам
        /// </summary>
        /// <param name="obj">Сравниваемый элемент</param>
        /// <returns>true, если равны и false в противном случае</returns>
        public override bool Equals(object obj)
        {
            IdentityItem<TId> other = obj as IdentityItem<TId>;
            return other == null ? false : this.id.Equals(other.id);
        }

        /// <summary>
        /// Возвращает хэш-код элемента по его идентификатору
        /// </summary>
        /// <returns>Хэш-код</returns>
        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        /// <summary>
        /// Сравнивает текущий и заданный элементы согласно их идентификаторам
        /// </summary>
        /// <param name="other">Сравниваемый элемент</param>
        /// <returns>true, если равны и false в противном случае</returns>
        public bool Equals(IdentityItem<TId> other)
        {
            return other == null ? false : this.id.Equals(other.id);
        }
    }
}
