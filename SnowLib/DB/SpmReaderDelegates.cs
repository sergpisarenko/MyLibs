using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SnowLib.DB
{
    /// <summary>
    /// Делегаты - ссылки на методы SqlDataReader 
    /// чтения полей по типам и индексу колонки
    /// </summary>
    internal class SpmReaderDelegates
    {
        private void addDelegate<TValue>(string method)
        {
            this.delegates.Add(typeof(TValue),
                typeof(SqlDataReader).GetMethod(method)
                .CreateDelegate(typeof(Func<SqlDataReader, int, TValue>)));
        }

        private readonly Dictionary<Type, Delegate> delegates;

        private SpmReaderDelegates()
        {
            this.delegates = new Dictionary<Type, Delegate>(15);
            addDelegate<int>("GetInt32");
            addDelegate<string>("GetString");
            addDelegate<decimal>("GetDecimal");
            addDelegate<float>("GetFloat");
            addDelegate<double>("GetDouble");
            addDelegate<Guid>("GetGuid");
            addDelegate<bool>("GetBoolean");
            addDelegate<byte>("GetByte");
            addDelegate<short>("GetInt16");
            addDelegate<long>("GetInt64");
            addDelegate<DateTime>("GetDateTime");
            addDelegate<TimeSpan>("GetTimeSpan");
            this.delegates.Add(typeof(byte[]), 
                new Func<SqlDataReader, int, byte[]>(readBytes));
            this.delegates.Add(typeof(ulong),
                new Func<SqlDataReader, int, ulong>(readUlong));
        }

        private static byte[] readBytes(SqlDataReader reader, int index)
        {
            return (byte[])reader[index];
        }

        private static ulong readUlong(SqlDataReader reader, int index)
        {
            byte[] raw = (byte[])reader[index];
            if (raw.Length!=8)
                throw new InvalidCastException(String.Concat("Поле \"",  reader.GetName(index), 
                    "\" должно содержать 8 байт для преобразования в ulong. Фактический размер: ", raw.Length));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(raw);
            return BitConverter.ToUInt64(raw, 0);
        }

        public Delegate GetDelegate(Type type)
        {
            Delegate d = null;
            this.delegates.TryGetValue(type, out d);
            return d;
        }

        private static SpmReaderDelegates instance;

        public static Delegate Get(Type type)
        {
            if (instance == null)
                instance = new SpmReaderDelegates();
            return instance.GetDelegate(type);
        }
    }
}
