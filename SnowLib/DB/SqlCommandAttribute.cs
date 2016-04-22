using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SnowLib.DB
{
    /// <summary>
    /// В интерфейсе задает вызов заданной команды Sql и хранимой процедуры
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SqlCommandAttribute : Attribute
    {
        public const int DefaultTimeout = 10;
        public readonly string CommandText;
        public readonly CommandType CommandType;
        public readonly int CommandTimeout;

        public SqlCommandAttribute(
            CommandType commandType = CommandType.StoredProcedure,
            string commandText = null, int commandTimeout = DefaultTimeout)
        {
            this.CommandType = commandType;
            this.CommandText = commandText;
            this.CommandTimeout = commandTimeout;
        }
    }
}
