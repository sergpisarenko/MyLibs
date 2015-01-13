using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SnowLib.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// Get description of the enum, added via attribure "Description"
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>Description or null if not exists</returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attr = null;
            if (value != null)
            {
                Type type = value.GetType();
                FieldInfo field = type.GetField(Enum.GetName(type, value));
                if (field != null)
                    attr = Attribute.GetCustomAttribute(field,  typeof(DescriptionAttribute)) as DescriptionAttribute;
            }
            return attr == null ? null : attr.Description;
        }
        
    }
}
