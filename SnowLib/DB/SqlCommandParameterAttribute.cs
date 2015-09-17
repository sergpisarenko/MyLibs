using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SnowLib.DB
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class SqlCommandParameterAttribute : Attribute
    {
        public readonly string Name;
        public readonly int Size;

        public SqlCommandParameterAttribute(
            string name = null, int size = 0)
        {
            this.Name = name;
            this.Size = size;
        }
    }
}
