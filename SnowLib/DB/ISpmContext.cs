using System;
using System.Data.SqlClient;

namespace SnowLib.DB
{
    internal interface ISpmContext
    {
        SqlDataReader DataReader { get; }
        int TryGetIndex(string columnName);
        object this[int propertyToken] { get; set; }
        SpmSharedItemPool SharedPool { get; }
    }
}
