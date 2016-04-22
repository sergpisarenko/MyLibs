using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SnowLib.Logging
{
    /// <summary>
    /// Вид сообщения
    /// </summary>
    public enum LogSeverity : byte
    {
        /// <summary>
        /// Отладочное
        /// </summary>
        Debug = 0, 

        /// <summary>
        /// Информационное
        /// </summary>
        Info = 1,  

        /// <summary>
        /// Предупреждающее
        /// </summary>
        Warn = 2, 

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        Error = 3
    }
}
