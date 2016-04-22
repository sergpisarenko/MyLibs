using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SnowLib.DB
{
    /// <summary>
    /// Класс для чтения размеченных элементов из запроса и
    /// предоставления других функций, обеспечиваемых разметкой
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    public sealed class SpmReader<TItem> : ISpmContext where TItem : new()
    {
        #region Закрытые поля
        private readonly Dictionary<string, int> indices;
        private readonly SpmItem<TItem> itemReader;
        private readonly SqlDataReader dataReader;
        private Dictionary<int, object> cache;
        private SpmSharedItemPool sharedPool;
        #endregion

        public SqlDataReader DataReader { get { return this.dataReader; } }

        public object this[int propertyToken]
        {
            get
            {
                object res = null;
                if (this.cache != null)
                    this.cache.TryGetValue(propertyToken, out res);
                return res;
            }
            set
            {
                if (this.cache == null)
                    this.cache = new Dictionary<int, object>(10);
                this.cache[propertyToken] = value;
            }
        }

        public SpmSharedItemPool SharedPool
        {
            get
            {
                if (this.sharedPool == null)
                    this.sharedPool = new SpmSharedItemPool();
                return this.sharedPool;
            }
        }

        public SpmReader(SqlDataReader reader, SpmSharedItemPool pool = null, string group = null)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            this.dataReader = reader;
            this.indices = new Dictionary<string, int>(this.DataReader.FieldCount);
            for (int i = 0; i < this.DataReader.FieldCount; i++)
            {
                string name = this.DataReader.GetName(i);
                if (indices.ContainsKey(name))
                    throw new ArgumentException(String.Concat("Колонка с именем ",
                        name, " встречается в запросе более одного раза"));
                indices.Add(name, i);
            }
            this.itemReader = new SpmItem<TItem>(this, null, group);
            this.sharedPool = pool;
        }

        public int TryGetIndex(string columnName)
        {
            int index;
            return this.indices.TryGetValue(columnName, out index) ? index : -1;
        }

        private void resetDataReader()
        {
            if (this.DataReader != null)
            {
                this.dataReader.Close();
                this.dataReader.Dispose();
            }
        }

        /// <summary>
        /// Получить все элементы в виде последовательности и закрыть запрос
        /// </summary>
        /// <returns>Последовательность</returns>
        public IEnumerable<TItem> Read()
        {
            try
            {
                while (this.DataReader.Read())
                    yield return itemReader.Read();
            }
            finally
            {
                resetDataReader();
            }
        }

        /// <summary>
        /// Загрузить свойства в элемент item, элемент должен быть создан, 
        /// а операция reader.Read() должна быть выполнена перед вызовом 
        /// этого метода, запрос остается открытым
        /// </summary>
        /// <param name="item">Целевой элемент</param>
        public void Read(TItem item)
        {
            itemReader.Read(item);
        }

        /// <summary>
        /// Выполнить reader.Read() и преобразовать считанную строку, 
        /// после чего закрыть запрос; элемент item должен быть создан перед вызовом.
        /// </summary>
        /// <param name="item">Целевой элемент</param>
        public void ReadOnce(TItem item)
        {
            try
            {
                if (!this.DataReader.Read())
                    throw new InvalidOperationException(String.Concat(
                        "Нет строк для чтения элемента \"", typeof(TItem).Name, "\""));
                itemReader.Read(item);
            }
            finally
            {
                resetDataReader();
            }
        }

        /// <summary>
        /// Получить все элементы в виде последовательности и закрыть запрос
        /// </summary>
        /// <param name="reader">Открытый запрос</param>
        /// <param name="pool">Пул разделяемых объектов</param>
        /// <returns>Последовательность</returns>
        public static object ReadEnumerable(SqlDataReader reader, SpmSharedItemPool pool = null)
        {
            SpmReader<TItem> spm = new SpmReader<TItem>(reader, pool);
            return spm.Read();
        }

        /// <summary>
        /// Получить все элементы в виде последовательности и закрыть запрос
        /// </summary>
        /// <param name="reader">Открытый запрос</param>
        /// <param name="pool">Пул разделяемых объектов</param>
        /// <returns>Последовательность</returns>
        public static IEnumerable<TItem> Read(SqlDataReader reader, SpmSharedItemPool pool = null)
        {
            SpmReader<TItem> spm = new SpmReader<TItem>(reader, pool);
            return spm.Read();
        }

        /// <summary>
        /// Загрузить свойства в элемент item, элемент должен быть создан, 
        /// а операция reader.Read() должна быть выполнена перед вызовом 
        /// этого метода, запрос остается открытым
        /// </summary>
        /// <param name="reader">Открытый запрос</param>
        /// <param name="item">Целевой элемент</param>
        /// <param name="group">Наименое группы размеченных свойств, 
        /// если null, то все свойства</param>
        /// <param name="pool">Пул разделяемых объектов</param>
        public static void Read(SqlDataReader reader, 
            TItem item, string group = null, SpmSharedItemPool pool = null)
        {
            SpmReader<TItem> spm = new SpmReader<TItem>(reader, pool, group);
            spm.Read(item);
        }

        /// <summary>
        /// Выполнить reader.Read() и преобразовать считанную строку, 
        /// после чего закрыть запрос; элемент item должен быть создан перед вызовом.
        /// </summary>
        /// <param name="reader">Открытый запрос</param>
        /// <param name="item">Целевой элемент</param>
        /// <param name="group">Наименое группы размеченных свойств, 
        /// если null, то все свойства</param>
        /// <param name="pool">Пул разделяемых объектов</param>
        public static void ReadOnce(SqlDataReader reader, 
            TItem item, string group = null, SpmSharedItemPool pool = null)
        {
            SpmReader<TItem> spm = new SpmReader<TItem>(reader, pool, group);
            spm.ReadOnce(item);
        }

        /// <summary>
        /// Скопировать размеченные свойства элемента source в элементе dest
        /// </summary>
        /// <param name="source">Исходные элемент</param>
        /// <param name="dest">Целевой элемент</param>
        /// <param name="group">Группа, если null, то все размеченные свойства</param>
        public static void Copy(TItem source, TItem dest, string group = null)
        {
            ISpmUpdateProperties write = dest as ISpmUpdateProperties;
            ReadOnlyCollection<ISpmAccessor<TItem>> coll = SpmAccessors<TItem>.Collection;
            if (write != null)
                write.BeginUpdateProperties();
            for (int i = 0; i < coll.Count; i++)
                if (coll[i].Mapped.InGroup(group))
                    coll[i].Copy(source, dest);
            if (write != null)
                write.EndUpdateProperties();
        }

        /// <summary>
        /// Сравнить еа павенство свойства элемента x и элемента y
        /// </summary>
        /// <param name="source">Исходные элемент</param>
        /// <param name="dest">Целевой элемент</param>
        /// <param name="group">Группа, если null, то все размеченные свойства</param>
        public static bool IsEquals(TItem x, TItem y, string group = null)
        {
            bool res = true;
            ReadOnlyCollection<ISpmAccessor<TItem>> coll = SpmAccessors<TItem>.Collection;
            for (int i = 0; i < coll.Count && res; i++ )
                if (coll[i].Mapped.InGroup(group))
                    res = coll[i].IsEquals(x, y);
            return res;
        }

    }
}
