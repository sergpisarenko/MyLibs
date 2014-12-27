using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Sql;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Configuration;

namespace SnowLib.DB
{
    /// <summary>
    /// MS SQL procedures exectuion helper
    /// </summary>
    /// <remarks>Copyright (c) 2014 PSN</remarks>
    public class SqlProc
    {
        /// <summary>
        /// Execution TimeOut
        /// </summary>
        public static int CommandTimeout = 60;
        // call prefix for CommandType.Text
        public const char TextTypePrefix = '@';
        // error code ID
        private const string ErrorCode = "ErrorCode";

        /// <summary>
        /// Заполняет датасет через вызов возвращающей выборки (SELECT) хранимой процедуры
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Заполненная таблица или null, если выборка отсутствует</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static DataSet FillDataSet(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            DataSet result = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                da = new SqlDataAdapter(cmd);
                result = new DataSet();
                cmd.Connection.Open();
                da.Fill(result);
                return result;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                if (result != null)
                    result.Dispose();
                throw;
            }
            finally
            {
                if (da != null)
                    da.Dispose();
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Заполняет таблицу через вызов возвращающей выборку (SELECT) хранимой процедуры
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Заполненная таблица или null, если выборка отсутствует</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static DataTable FillTable(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            DataSet result = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                da = new SqlDataAdapter(cmd);
                result = new DataSet();
                cmd.Connection.Open();
                da.Fill(result);
                if (result.Tables.Count > 0)
                {
                    DataTable dt = result.Tables[0];
                    result.Tables.RemoveAt(0);
                    return dt;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                if (result != null)
                    result.Dispose();
                throw;
            }
            finally
            {
                if (da != null)
                    da.Dispose();
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Заполняет таблицу через вызов возвращающей выборку (SELECT) хранимой процедуры,
        /// при этом можно передавать несколько выходных параметров
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procCmd">Созданная Sql-команда, откуда можно прочитать значеные выходных параметров</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Заполненная таблица или null, если выборка отсутствует</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static DataTable FillTable(string connTag, string procName, 
            out SqlCommand procCmd, params object[] procParams)
        {
            procCmd = null;
            SqlDataAdapter da = null;
            DataSet result = null;
            try
            {
                procCmd = CreateCommand(connTag, procName);
                AddParams(procCmd, procParams);
                da = new SqlDataAdapter(procCmd);
                result = new DataSet();
                procCmd.Connection.Open();
                da.Fill(result);
                if (result.Tables.Count > 0)
                {
                    DataTable dt = result.Tables[0];
                    result.Tables.RemoveAt(0);
                    return dt;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                ProcessException(procCmd, ex);
                if (result != null)
                    result.Dispose();
                throw;
            }
            finally
            {
                if (da != null)
                    da.Dispose();
                ProcessClose(procCmd, false);
            }
        }

        /// <summary>
        /// Создает SqlDataReader через вызов возвращающей выборку (SELECT) хранимой процедуры
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>SqlDataReader, после его использования он должен быть закрыт через метод
        /// Close(), который автоматически закроет и само соединение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static SqlDataReader ExecuteReader(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                cmd.Connection.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                ProcessClose(cmd, true);
                throw;
            }
        }

        /// <summary>
        /// Создает SqlDataReader через вызов возвращающей выборку (SELECT) хранимой процедуры
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procCmd">Созданная Sql-команда, откуда можно прочитать значеные выходных параметров</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>SqlDataReader, после его использования он должен быть закрыт через метод
        /// Close(), который автоматически закроет и само соединение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static SqlDataReader ExecuteReader(string connTag, string procName,
            out SqlCommand procCmd, params object[] procParams)
        {
            procCmd = null;
            try
            {
                procCmd = CreateCommand(connTag, procName);
                AddParams(procCmd, procParams);
                procCmd.Connection.Open();
                return procCmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                ProcessException(procCmd, ex);
                ProcessClose(procCmd, true);
                throw;
            }
        }


        /// <summary>
        /// Возвращает одну строку в виде словаря имя-значение 
        /// для возвращающей выборку (SELECT) хранимой процедуры
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="throwNoData">
        /// Генерировать ли исключение, если результат пустой (ни одной строки не получено)
        /// </param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>SqlDataReader, после его использования он должен быть закрыт через метод
        /// Close(), который автоматически закроет и само соединение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static Dictionary<string,object> GetFirstRow(string connTag, string procName, bool throwNoData, params object[] procParams)
        {
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            Dictionary<string, object> res = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    res = new Dictionary<string, object>(dr.FieldCount);
                    for (int i = 0; i < dr.FieldCount; i++)
                        res[dr.GetName(i)] = dr[i];
                }
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                if (dr != null)
                    dr.Close();
                ProcessClose(cmd, false);
            }
            if (res == null && throwNoData)
                throw new SqlNotFilledException("Нет данных.");
            return res;
        }


        /// <summary>
        /// Вызывает хранимую процедуру, не выполняющую выборку данных (Add, Alter, Del)
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое процедурой значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static int ExecuteNonQuery(string connTag, string procName, params object[] procParams )
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return (int)cmd.Parameters["@RETURN_VALUE"].Value;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, не выполняющую выборку данных (Add, Alter, Del),
        /// в процедуру можно передать один выходной параметр
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procOutParValue">Ссылка на значение выходного параметра</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое процедурой значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static int ExecuteNonQuery(string connTag, string procName, 
            out object procOutParValue, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                List<SqlParameter> parsOut = AddParams(cmd, procParams);
                if (parsOut.Count != 1)
                    throw new ArgumentException("Only one out parameter must be set");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                procOutParValue = parsOut[0].Value;
                return (int)cmd.Parameters["@RETURN_VALUE"].Value;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, не выполняющую выборку данных (Add, Alter, Del),
        /// в процедуру можно передать несколько выходных параметров
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procCmd">Созданная Sql-команда, откуда можно прочитать значеные выходных параметров</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое процедурой значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static int ExecuteNonQuery(string connTag, string procName,
            out SqlCommand procCmd, params object[] procParams)
        {
            procCmd = null;
            try
            {
                procCmd = CreateCommand(connTag, procName);
                List<SqlParameter> parsOut = AddParams(procCmd, procParams);
                if (parsOut.Count != 1)
                    throw new ArgumentException("Only one out parameter must be set");
                procCmd.Connection.Open();
                procCmd.ExecuteNonQuery();
                return (int)procCmd.Parameters["@RETURN_VALUE"].Value;
            }
            catch (Exception ex)
            {
                ProcessException(procCmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(procCmd, false);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, добавляющую записи с ключом типа SqlDbType.Int,
        /// имя ключа передается через выходной параметр соответствующего типа
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Значение выходного параметра</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static int ExecuteNonQueryInt(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                List<SqlParameter> parsOut = AddParams(cmd, procParams);
                if (parsOut.Count != 1)
                    throw new ArgumentException("Only one out parameter must be set");
                if (parsOut[0].SqlDbType!=SqlDbType.Int )
                    throw new ArgumentException("Out parameter must have SqlDbType.Int type");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                object value = parsOut[0].Value;
                return (int)value;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, добавляющую записи с ключом типа SqlDbType.BigInt,
        /// имя ключа передается через выходной параметр соответствующего типа
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Значение выходного параметра</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static long ExecuteNonQueryLong(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                List<SqlParameter> parsOut = AddParams(cmd, procParams);
                if (parsOut.Count != 1)
                    throw new ArgumentException("Only one out parameter must be set");
                if (parsOut[0].SqlDbType != SqlDbType.BigInt)
                    throw new ArgumentException("Out parameter must have SqlDbType.BigInt type");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                object value = parsOut[0].Value;
                return (long)value;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, выполняющую выборку данных скалярных данных
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое процедурой значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static object ExecuteScalar(string connTag, string procName, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, procName);
                AddParams(cmd, procParams);
                cmd.Connection.Open();
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Вызывает хранимую процедуру, выполняющую выборку данных скалярных данных
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой процедуры</param>
        /// <param name="procCmd">Созданная Sql-команда, откуда можно прочитать значеные выходных параметров</param>
        /// <param name="procParams">Параметры хранимой процедуры в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое процедурой значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static object ExecuteScalar(string connTag, string procName,
            out SqlCommand procCmd, params object[] procParams)
        {
            procCmd = null;
            try
            {
                procCmd = CreateCommand(connTag, procName);
                AddParams(procCmd, procParams);
                procCmd.Connection.Open();
                return procCmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                ProcessException(procCmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(procCmd, false);
            }
        }

        /// <summary>
        /// Вызывает хранимую функцию
        /// </summary>
        /// <param name="connTag">Имя тэга из config-секции ConnectionStrings или 
        /// непосредственно строка подключения целиком</param>
        /// <param name="procName">Имя хранимой функция</param>
        /// <param name="procParams">Параметры хранимой функции в виде имя:значение:Sql тип</param>
        /// <returns>Возвращаемое функцией значение</returns>
        /// <exception cref="System.SqlException">Возникает при ошибках обращения к SQL-серверу,
        /// в поле Data (int)Data["ErrorCode"] содержит дополнительно код ошибки</exception>
        /// <exception cref="System.ArgumentException">Неверный аргумент</exception>
        /// <exception cref="System.ArgumentNullException">Аргумент не может быть null</exception>
        public static object ExecuteFunction(string connTag, string funcName, params object[] procParams)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateCommand(connTag, funcName);
                AddParams(cmd, procParams);
                string resultName;
                if (funcName[0] == TextTypePrefix)
                {
                    resultName = "@FUNCTION_RESULT";
                    SqlParameter par = new SqlParameter(resultName, SqlDbType.Variant);
                    par.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(par);
                    cmd.CommandText = "EXEC " + resultName + "=" + cmd.CommandText;
                }
                else
                {
                    resultName = "@RETURN_VALUE";
                }
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return cmd.Parameters[resultName].Value;
            }
            catch (Exception ex)
            {
                ProcessException(cmd, ex);
                throw;
            }
            finally
            {
                ProcessClose(cmd, true);
            }
        }

        /// <summary>
        /// Возвращает код ошибки для Sql-исключения и 0 для все остальных
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <returns>Код для Sql-исключения и 0 для остальных</returns>
        public static int GetErrorCode( Exception ex )
        {
            if (ex.Data[ErrorCode] != null)
                return (int)ex.Data[ErrorCode];
            else
                return 0;
        }

        /// <summary>
        /// Возвращает номер ошибки для Sql-исключения и 0 для все остальных
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <returns>Номер для Sql-исключения и 0 для остальных</returns>
        public static int GetErrorNumber(Exception ex)
        {
            if (ex is SqlException)
                return ((SqlException)ex).Number;
            else
                return 0;
        }

        /// <summary>
        /// Освобождает ресурсы, занимаемые объектов с интерфейсом IDisposable,
        /// если он не null
        /// </summary>
        /// <param name="obj">Освобождаемый объект</param>
        public static void Dispose(IDisposable obj)
        {
            if (obj != null)
                obj.Dispose();
        }

        // создает команду для вызова хранимой процедуры
        private static SqlCommand CreateCommand(string connectionString, string procName)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");
            if (String.IsNullOrEmpty(procName))
                throw new ArgumentNullException("procName");
            CommandType cmdType;
            if (procName[0] == TextTypePrefix)
            {
                // call procedure in text 'EXEC procname ...'
                // for linked servers because a problem
                // http://support.microsoft.com/kb/969190
                procName = procName.TrimStart(TextTypePrefix);
                cmdType = CommandType.Text;
            }
            else
            {
                // casual procedure call
                cmdType = CommandType.StoredProcedure;
            }
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(procName, conn);
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = CommandTimeout;
            return cmd;
        }

        // добавляет параметры для вызова хранимой процедуры
        private static List<SqlParameter> AddParams(SqlCommand procCmd, object[] procParams )
        {
            if (procCmd == null)
                throw new ArgumentNullException("Sql command can't be null");
            if (procParams == null)
                throw new ArgumentNullException("Parameters array can't be null");
            if ((procParams.Length % 3) > 0)
                throw new ArgumentException("Wrong parameters format (some elements may be lost): must be Name:Value:SqlType for each parameter");
            SqlParameter param;
            List<SqlParameter> parsOut = new List<SqlParameter>(procParams.Length / 3);
            StringBuilder sbPars = 
                procCmd.CommandType==CommandType.StoredProcedure?null:new StringBuilder(" ");
            for (int i = 0; i < procParams.Length; i += 3)
            {
                object objName = procParams[i];
                object objValue = procParams[i + 1];
                if (objName == null)
                    throw new ArgumentException("Parameter name can't be null");
                bool bnull;
                if (bnull = objValue == null || String.Empty.Equals(objValue))
                    objValue = DBNull.Value;
                StringBuilder name = new StringBuilder( objName.ToString().Trim() );
                if (name.Length==0)
                    throw new ArgumentException("Parameter name can't be empty");
                ParameterDirection pdir;
                if (name[0]=='&') // признак out-параметра
                {
                    name[0] = '@';
                    if (bnull)
                        pdir = ParameterDirection.Output;
                    else
                        pdir = ParameterDirection.InputOutput;
                }
                else
                {
                    name.Insert(0, '@');
                    pdir = ParameterDirection.Input;
                }
                param = new SqlParameter(name.ToString(), objValue);
                param.SqlDbType = (SqlDbType)procParams[i + 2];
                param.Direction = pdir;
                procCmd.Parameters.Add(param);
                if (param.Direction != ParameterDirection.Input)
                    parsOut.Add(param);
                if (sbPars != null)
                {
                    sbPars.Append(name.ToString());
                    sbPars.Append('=');
                    sbPars.Append(name.ToString());
                    sbPars.Append(',');
                }
            }
            param = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            param.Direction = ParameterDirection.ReturnValue;
            procCmd.Parameters.Add( param );
            if (sbPars != null && sbPars.Length>1)
            {
                sbPars.Remove(sbPars.Length-1, 1);
                procCmd.CommandText += sbPars.ToString();
            }
            return parsOut;
        }

        // обрабатывает исключение
        private static void ProcessException(SqlCommand procCmd, Exception ex)
        {
            if (ex is SqlException)
            {
                SqlException exSql = (SqlException)ex;
                // пользовательский код ошибки (Number=50000), возвращается через ReturnValue
                exSql.Data[ErrorCode] = (exSql.Number == 50000) ?
                    procCmd.Parameters["@RETURN_VALUE"].Value : exSql.Number;
            }
        }

        // закрывает соединение и команду
        private static void ProcessClose( SqlCommand procCmd, bool disposeCmd )
        {
            if (procCmd != null)
            {
                if (procCmd.Connection != null)
                    procCmd.Connection.Close();
                if ( disposeCmd )
                    procCmd.Dispose();
            }
        }
    }
}
