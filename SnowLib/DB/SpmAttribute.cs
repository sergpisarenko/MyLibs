using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using SnowLib.Extensions;

namespace SnowLib.DB
{
    /// <summary>
    /// Атрибут, помечащюий свойство как автоматически считываевемое из SqlDataReader
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SpmAttribute : Attribute
    {
        public string ColumnNames
        {
            get { return String.Join(", ", this.columns); }
            set { this.columns = value.SplitTrim(','); }
        }

        /// <summary>
        /// Ключевое поле - поле, по которым происходит сравнение 
        /// в методах Equals и получение хэш-кода GetHashCode
        /// Может быть несколько ключевых полей
        /// </summary>
        public bool Key { get; set; }

        public string Groups
        {
            get { return String.Join(", ", this.propertyGroups); }
            set { this.propertyGroups = value.SplitTrim(','); }
        }

        /// <summary>
        /// Если задано ShareId, то свойство разделяемое. 
        /// Применяется для ссылочных
        /// пользовательсих типом, когда необходимо, чтобы
        /// в имелась ссылка на один тот же объект для
        /// одинаковых ключевых полей. Реализуется посредством
        /// словаря, необходима корректная поддержка методов
        /// Equals и GetHashCode по ключевым полям
        /// </summary>
        public string ShareId
        {
            get { return this.shareId; }
            set { this.shareId = value; }
        }

        private StringDictionary dictSynonyms;
        /// <summary>
        /// Синонимы имен полей
        /// </summary>
        public string Synonyms
        {
            get
            {
                if (this.dictSynonyms == null)
                    return null;
                else
                {
                    StringBuilder sb = new StringBuilder(this.dictSynonyms.Count*16);
                    string div = String.Empty;
                    foreach(DictionaryEntry entry in this.dictSynonyms)
                    {
                        sb.Append(div);
                        sb.Append(entry.Key);
                        sb.Append('=');
                        sb.Append(entry.Value);
                        div = ", ";
                    }
                    return sb.ToString();
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    this.dictSynonyms = null;
                }
                else
                {
                    string[] syns = value.SplitTrim(',');
                    this.dictSynonyms = new StringDictionary();
                    foreach (string pair in syns)
                    {
                        string[] nameSyn = pair.Split('=');
                        if (nameSyn.Length != 2 || nameSyn[0].Length == 0 || nameSyn[1].Length == 0)
                            throw new ArgumentException("Некорректное определение синонимов. Должно быть: имя=сионинм; имя=синоним; ...");
                        this.dictSynonyms.Add(nameSyn[0].Trim(), nameSyn[1].Trim());
                    }
                }

            }
        }

        private string[] propertyGroups = StringEx.EmptyArray;

        private string shareId = String.Empty;

        private string[] columns = StringEx.EmptyArray;

        internal string[] Columns
        {
            get { return this.columns; }
        }

        internal bool Shared { get { return !String.IsNullOrEmpty(this.shareId); } }

        internal bool HasSynonyms { get { return this.dictSynonyms != null; } }

        internal string GetSynonim(string name)
        {
            return this.dictSynonyms[name];
        }

        internal bool InGroup(string group)
        {
            return String.IsNullOrEmpty(group) ||
                Array.IndexOf(this.propertyGroups, group) >= 0;
        }
    }
}
