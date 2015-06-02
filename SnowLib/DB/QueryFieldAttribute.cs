using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.DB
{
    public class QueryFieldAttribute : Attribute
    {
        public readonly string Name;

        public QueryFieldAttribute(string name)
        {
            this.Name = name;
        }

    }
}
