using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.DB
{
    /// <summary>
    /// Тип для представления возвращаемого при помощи 
    /// RETURN из хранимой процедуры целого значения 
    /// </summary>
    public struct SqlReturn
    {
        public readonly int Value;

        public SqlReturn(int value)
        {
            this.Value = value;
        }
    }
}
