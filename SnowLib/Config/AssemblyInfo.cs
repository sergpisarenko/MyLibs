using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Configuration;

namespace SnowLib.Config
{
    /// <summary>
    /// Класс для описания сборки и путей хранениия
    /// конфигурационных и журнальных файлов
    /// </summary>
    public class AssemblyInfo
    {
        private const string configFileName = "Config.xml";
        private const string logSubFolder = "Logs";

        /// <summary>
        /// Исходная сборка
        /// </summary>
        public readonly Assembly Source;
        /// <summary>
        /// Наименование (файла) сборки
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Наименование сборки (описательное)
        /// </summary>
        public readonly string Title;
        /// <summary>
        /// Описание сборки
        /// </summary>
        public readonly string Description;
        /// <summary>
        /// Версия сборки
        /// </summary>
        public readonly string Version;
        /// <summary>
        /// Рабочая подпапка (на основе Name)
        /// </summary>
        public readonly string SubFolder;

        public AssemblyInfo(Assembly assembly)
        {
            this.Source = assembly;
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            this.Name = assembly.GetName().Name;
            AssemblyTitleAttribute attrTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            this.Title = attrTitle == null ? this.Name : attrTitle.Title;
            AssemblyDescriptionAttribute attrDesc = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            this.Description = attrDesc == null ? String.Empty : attrDesc.Description;
            this.SubFolder = this.Name.Replace('.', Path.DirectorySeparatorChar);
            AssemblyFileVersionAttribute attrVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            this.Version = attrVersion == null ? String.Empty : attrVersion.Version;
        }

        public override string ToString()
        {
            return this.Title.ToString();
        }

        /// <summary>
        /// Возвращает базовый путь к рабочим файлам приложения
        /// </summary>
        /// <param name="userLevel">Уровень конфигурации - локальный компьютер, 
        /// локальный пользователь, перемещаемый пользователь</param>
        /// <returns>Путь к рабочим файлам</returns>
        public string GetDataPath(ConfigurationUserLevel userLevel)
        {
            Environment.SpecialFolder specialFolder;
            switch(userLevel)
            {
                case ConfigurationUserLevel.None:
                    specialFolder = Environment.SpecialFolder.CommonApplicationData;
                    break;
                case ConfigurationUserLevel.PerUserRoamingAndLocal:
                    specialFolder = Environment.SpecialFolder.LocalApplicationData;
                    break;
                case ConfigurationUserLevel.PerUserRoaming:
                    specialFolder = Environment.SpecialFolder.ApplicationData;
                    break;
                default:
                    throw new ArgumentException("userLevel");
            }
            return Path.Combine(Environment.GetFolderPath(specialFolder), this.SubFolder);
        }

        /// <summary>
        /// Возвращает текущую конфигурацию приложения
        /// </summary>
        /// <param name="userLevel">Уровень конфигурации - локальный компьютер, 
        /// локальный пользователь, перемещаемый пользователь</param>
        /// <returns>Конфигурация</returns>
        public Configuration GetConfiguration(ConfigurationUserLevel userLevel)
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.RoamingUserConfigFilename = Path.Combine(GetDataPath(ConfigurationUserLevel.PerUserRoaming), configFileName);
            map.LocalUserConfigFilename = Path.Combine(GetDataPath(ConfigurationUserLevel.PerUserRoamingAndLocal), configFileName);
            map.ExeConfigFilename = Path.Combine(GetDataPath(ConfigurationUserLevel.None), configFileName);
            return ConfigurationManager.OpenMappedExeConfiguration(map, userLevel);
        }

        /// <summary>
        /// Возвращает путь к журналам приложения
        /// </summary>
        /// <param name="userLevel">Уровень конфигурации - локальный компьютер, 
        /// локальный пользователь, перемещаемый пользователь</param>
        /// <returns>Путь к журналам приложения</returns>
        public string GetLogPath(ConfigurationUserLevel userLevel)
        {
            return Path.Combine(GetDataPath(userLevel), logSubFolder);
        }
    }
}
