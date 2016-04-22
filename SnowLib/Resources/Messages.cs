using System;
using System.Resources;
using System.Globalization;

namespace SnowLib
{
    /// <summary>
    /// Работа с текстовыми ресурсами (строками)
    /// </summary>
    internal static class Messages
    {
        private static ResourceManager resourceManager;

        static Messages()
        {
            Type type = typeof(Messages);
            resourceManager = new ResourceManager(type.Namespace + ".Resources." + type.Name, type.Assembly);
        }

        internal static string Get(string name)
        {
            return resourceManager.GetString(name, CultureInfo.CurrentCulture);
        }

        internal static string Get(string name, CultureInfo cultureInfo)
        {
            return resourceManager.GetString(name, cultureInfo);
        }

        internal static string Format(string name, params object[] args)
        {
            return String.Format(resourceManager.GetString(name, CultureInfo.CurrentCulture), args);
        }

        internal static string Format(string name, CultureInfo cultureInfo, params object[] args)
        {
            return String.Format(resourceManager.GetString(name, cultureInfo), args);
        }

    }
}
