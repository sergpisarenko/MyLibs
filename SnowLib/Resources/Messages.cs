using System;
using System.Globalization;

namespace SnowLib
{
    internal static class Messages
    {

        private static System.Resources.ResourceManager resourceMan;

        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("SnowLib.Resources.Messages", typeof(Messages).Assembly);
                return resourceMan;
            }
        }

        internal static string Get(string name)
        {
            return ResourceManager.GetString(name, CultureInfo.CurrentCulture);
        }

        internal static string Get(string name, CultureInfo culture)
        {
            return ResourceManager.GetString(name, culture);
        }
    }
}
