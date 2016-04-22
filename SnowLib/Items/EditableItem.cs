using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SnowLib.Items
{
    /// <summary>
    /// Редактируемый элемент с идентификатором и наименованием
    /// </summary>
    /// <typeparam name="TId">Тип идентификатора</typeparam>
    [Serializable]
    [DataContract]
    public class EditableItem<TId> : 
        IdentityItem<TId>, 
        INotifyPropertyChanged, 
        SnowLib.DB.ISpmUpdateProperties
    {
        #region Закрытые поля
        // Наименование элемента
        [DataMember]
        protected string name;
        // битовые состояния 
        // 1 бит - признак обновления через SpmReader
        [DataMember]
        protected BitVector32 bools;
        #endregion

        #region Открытые поля
        /// <summary>
        /// Признак редактирования пользователем полей
        /// (false - массовое заполнение полей из БД или 
        /// другого источника)
        /// </summary>
        protected bool IsEditing
        {
            get { return !this.bools[1 << 31]; }
            private set { this.bools[1 << 31] = !value; }
        }
        #endregion

        #region Common base methods
        /// <summary>
        /// Текстовая информация об элементе
        /// </summary>
        /// <returns>Возвращает поле наименования элемента</returns>
        public override string ToString()
        {
            return this.name;
        }
        #endregion

        #region User property changing
        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.IsEditing)
            {
                PropertyChangedEventHandler handler = System.Threading.Volatile.Read(ref this.PropertyChanged);
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region ISpmUpdateProperties
        public virtual void BeginUpdateProperties()
        {
            IsEditing = false;
        }

        public virtual void EndUpdateProperties()
        {
            IsEditing = true;
            onPropertyChanged(null);
        }
        #endregion
    }
}
