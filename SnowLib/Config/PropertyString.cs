using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnowLib.Extensions;

namespace SnowLib.Config
{
    /// <summary>
    /// Класс для работы с конфигурационными настройками
    /// в виде строки вида "Наименование1=Значение; Наименование2=Значение2..."
    /// </summary>
    public class PropertyString
    {
        private readonly Dictionary<string, string> dictionary;

        /// <summary>
        /// Создание на основе строки настроек
        /// </summary>
        /// <param name="propertyString">Строка настроек</param>
        public PropertyString(string propertyString)
        {
            this.dictionary = new Dictionary<string, string>(10);
            if (propertyString != null)
            {
                string[] assignments = propertyString.SplitTrim(';');
                for (int i = 0; i < assignments.Length; i++)
                {
                    string[] pv = assignments[i].Split('=');
                    if (pv.Length != 2)
                        throw new ArgumentOutOfRangeException(assignments[i]);
                    this.dictionary[pv[0].ToLower()] = pv[1];
                }
            }
        }

        /// <summary>
        /// Число найденных настроек
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Получить строку по имени параметра
        /// </summary>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="isMandatory">True, если строка должна быть задана и не быть пустой</param>
        /// <returns>Значаме параметра в виде строки</returns>
        /// <exception cref="System.ArgumentException">Если isMandatory=true, 
        /// а параметра нет или значение его пустое</exception>
        public string GetString(string propertyName, bool isMandatory)
        {
            string value;
            if (this.dictionary.TryGetValue(propertyName.ToLower(), out value) && !String.IsNullOrEmpty(value))
                return value;
            else
            {
                if (isMandatory)
                    throw new ArgumentException(propertyName);
                return String.Empty;
            }
        }

        /// <summary>
        /// Получить значение перечисления
        /// </summary>
        /// <typeparam name="T">Тип перечисления</typeparam>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="result">Значение перечисления</param>
        /// <returns>True, если было успешно получено и false в противном случае</returns>
        public bool GetEnum<T>(string propertyName, out T result) where T : struct
        {
            string value;
            if (this.dictionary.TryGetValue(propertyName.ToLower(), out value) && !String.IsNullOrEmpty(value))
                return Enum.TryParse<T>(value, out result);
            else
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Получить значение перечисления
        /// </summary>
        /// <typeparam name="T">Тип перечисления</typeparam>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="isMandatory">True, значение данного перечисления должно быть указано</param>
        /// <returns>Значение перечисления</returns>
        /// <exception cref="System.ArgumentException">Если isMandatory=true, 
        /// а параметра нет или значение его пустое</exception>
        public T GetEnum<T>(string propertyName, bool isMandatory) where T : struct
        {
            T result = default(T);
            if (!GetEnum<T>(propertyName, out result) && isMandatory)
                throw new ArgumentException(propertyName);
            return  result;
        }

        /// <summary>
        /// Получает значение интервала времени
        /// </summary>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="defaultValue">Значение по умолчанию,   </param>
        /// <returns>Интервал времени</returns>
        /// <exception cref="FormatException">Значение имеет недопустимый формат</exception>
        /// <exception cref="OverflowException">Значение интервала или его компонент вне диапазона</exception>
        public TimeSpan GetOptional(string propertyName, TimeSpan defaultValue)
        {
            string value;
            if (this.dictionary.TryGetValue(propertyName.ToLower(), out value) && !String.IsNullOrEmpty(value))
                return TimeSpan.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            else
                return defaultValue;
        }

        /// <summary>
        /// Получить целое число
        /// </summary>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="defaultValue">Значение по умолчанию, если такого переметра не найдено</param>
        /// <returns>Целое значение параметра</returns>
        /// <exception cref="FormatException">Значение имеет недопустимый формат</exception>
        /// <exception cref="OverflowException">Значение вне диапазона</exception>
        public int GetOptional(string propertyName, int defaultValue)
        {
            string value;
            if (this.dictionary.TryGetValue(propertyName.ToLower(), out value) && !String.IsNullOrEmpty(value))
                return int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            else
                return defaultValue;
        }

        /// <summary>
        /// Получить логическое значение
        /// </summary>
        /// <param name="propertyName">Имя параметра</param>
        /// <param name="defaultValue">Значение по умолчанию, если такого переметра не найдено</param>
        /// <returns>Логическое значение параметра</returns>
        /// <exception cref="FormatException">Значение имеет недопустимый формат</exception>
        public bool GetOptional(string propertyName, bool defaultValue)
        {
            string value;
            if (this.dictionary.TryGetValue(propertyName.ToLower(), out value) && !String.IsNullOrEmpty(value))
                return bool.Parse(value);
            else
                return defaultValue;
        }
    }
}
