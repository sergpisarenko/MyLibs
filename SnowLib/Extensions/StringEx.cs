using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;

namespace SnowLib.Extensions
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringEx
    {
        #region Константы и закрытые поля
        private static byte[] salt = { 1, 3, 4, 8, 3, 5, 4 };
        #endregion
        
        #region Public methods
        /// <summary>
        /// Шифрует строку и преобразует шифрованный массив байт в строку формата Base64. 
        /// Если userLevel = true, то при шифровании используется контекст защиты текущего 
        /// пользователя, иначе – компьютера.
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <param name="userLevel">Конекст защиты пользователя или компьютера</param>
        /// <returns>Зашифрованная строка в Base64</returns>
        public static string ToProtected(this string source, bool userLevel)
        {
            if (String.IsNullOrWhiteSpace(source))
                return null;
            else
                return Convert.ToBase64String(System.Security.Cryptography.ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(source), salt, userLevel ?
                    System.Security.Cryptography.DataProtectionScope.CurrentUser :
                    System.Security.Cryptography.DataProtectionScope.LocalMachine));
        }

        /// <summary>
        /// Расшифровывает строку, защищенную с помощью метода <ref>ToProtected</ref>
        /// </summary>
        /// <param name="source">Исходная зашифрованная строка в Base64</param>
        /// <param name="userLevel">Конекст защиты пользователя или компьютера</param>
        /// <returns>Расшифрованная строка</returns>
        public static string FromProtected(this string source, bool userLevel)
        {
            if (String.IsNullOrWhiteSpace(source))
                return null;
            else
                return Encoding.UTF8.GetString(System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(source), salt, userLevel ?
                    System.Security.Cryptography.DataProtectionScope.CurrentUser :
                    System.Security.Cryptography.DataProtectionScope.LocalMachine));
        }

        /// <summary>
        /// Преобразует строку в безопасную строку
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <returns>Безопасная строка</returns>
        public static SecureString ToSecure(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            else
            {
                SecureString result = new SecureString();
                for (int i = 0; i < source.Length; i++)
                    result.AppendChar(source[i]);
                result.MakeReadOnly();
                return result;
            }
        }

        /// <summary>
        /// Преобразует безопасную строку в обычную
        /// </summary>
        /// <param name="secureString">Исходная безопасная строка</param>
        /// <returns>Обычная строка</returns>
        public static string ToUnsecure(this SecureString secureString)
        {
            if (secureString == null)
                return String.Empty;
            IntPtr unmanagedStringPtr = IntPtr.Zero;
            try
            {
                unmanagedStringPtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedStringPtr);
            }
            catch
            {
                throw;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedStringPtr);
            }
        }

        /// <summary>
        /// Шифрует содержимое безопасной строки (SecureString) и преобразует 
        /// шифрованный массив байт в строку формата Base64. Если userLevel = true, 
        /// то при шифровании используется контекст защиты текущего пользователя, 
        /// иначе – компьютера.
        /// </summary>
        /// <param name="secureString">Безопасная строка</param>
        /// <param name="userLevel">Конекст защиты пользователя или компьютера</param>
        /// <returns>Зашифрованная строка в Base64</returns>
        public static string ToProtected(this SecureString secureString, bool userLevel)
        {
            if (secureString==null)
                return null;
            else
            {
                IntPtr unmanagedStringPtr = IntPtr.Zero;
                char[] unsecuredChars = new char[secureString.Length];
                byte[] unsecuredBuf = null;
                try
                {
                    unmanagedStringPtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
                    System.Runtime.InteropServices.Marshal.Copy(unmanagedStringPtr, unsecuredChars, 0, unsecuredChars.Length);
                    unsecuredBuf = Encoding.UTF8.GetBytes(unsecuredChars);
                    return Convert.ToBase64String(ProtectedData.Protect(unsecuredBuf, salt, userLevel ?
                        System.Security.Cryptography.DataProtectionScope.CurrentUser :
                        System.Security.Cryptography.DataProtectionScope.LocalMachine));
                }
                catch
                {
                    throw;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedStringPtr);
                    Array.Clear(unsecuredChars, 0, unsecuredChars.Length);
                    if (unsecuredBuf!=null)
                        Array.Clear(unsecuredBuf, 0, unsecuredBuf.Length);
                }
            }
        }

        /// <summary>
        /// Расшифровывает строку, защищенную с помощью метода <ref>ToProtected</ref> и 
        /// преобразует расшифрованную строку в безопасную (SecureString). userLevel 
        /// </summary>
        /// <param name="source">Исходная зашифрованная строка в Base64</param>
        /// <param name="userLevel">Определяет контекст защиты – текущий пользователь или компьютер.</param>
        /// <returns>Безопасная строка</returns>
        public static SecureString ProtectedToSecure(this string source, bool userLevel)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            else
            {
                byte[] unprotectedBuf = null;
                char[] unprotectedChars = null;
                try
                {
                    unprotectedBuf = ProtectedData.Unprotect(Convert.FromBase64String(source), salt, userLevel ?
                        System.Security.Cryptography.DataProtectionScope.CurrentUser :
                        System.Security.Cryptography.DataProtectionScope.LocalMachine);
                    unprotectedChars = Encoding.UTF8.GetChars(unprotectedBuf);
                    SecureString result = new SecureString();
                    Array.ForEach<char>(unprotectedChars, m => result.AppendChar(m));
                    result.MakeReadOnly();
                    return result;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (unprotectedBuf != null)
                        Array.Clear(unprotectedBuf, 0, unprotectedBuf.Length);
                    if (unprotectedChars != null)
                        Array.Clear(unprotectedChars, 0, unprotectedChars.Length);
                }
            }
        }

        /// <summary>
        /// Удаляет форматирование (множество пробелов, табуляции, переносы)
        /// и обрезает строку до нужной длины. При обрезке добавляет в конце «…».
        /// </summary>
        /// <param name="source">Исходная строка с форматированием</param>
        /// <param name="limit">Ограничение длины</param>
        /// <returns>Строка без форматированая нужной длины (+3 символа при обрезке)</returns>
        public static string UnformatAndTrim(this string source, int limit)
        {
            if (String.IsNullOrEmpty(source))
                return source;
            StringBuilder sb = new StringBuilder(limit);
            int index = 0;
            char prevChar = (char)0;
            while(index<source.Length && sb.Length<limit)
            {
                char currChar = source[index];
                if (Char.IsWhiteSpace(currChar))
                {
                    currChar = ' ';
                    if (prevChar != ' ')
                    {
                        sb.Append(currChar);
                        prevChar = currChar;
                    }
                }
                else
                {
                    sb.Append(currChar);
                    prevChar = currChar;
                }
                index++;
            }
            if (source.Length > sb.Length)
                sb.Append("...");
            return sb.ToString();
        }

        public static readonly string[] EmptyArray = new string[0];

        /// <summary>
        /// Разделяет строку на элементы согласно символу-разделителю, 
        /// при этом удаляет любое форматирование (множество пробелов, табуляции, переносы).
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <param name="separator">Символ-разделитель</param>
        /// <returns>Массив разделенных элементов без форматирования</returns>
        public static string[] SplitTrim(this string source, char separator)
        {
            if (Char.IsWhiteSpace(separator))
                throw new ArgumentException("separator is white-space");
            if (String.IsNullOrEmpty(source))
                return EmptyArray;
            List<string> res = new List<string>(8);
            StringBuilder sb = new StringBuilder(16);
            int index = 0;
            while (index < source.Length)
            {
                char c = source[index++];
                if (!char.IsWhiteSpace(c))
                {
                    if (c == separator)
                    {
                        if (sb.Length > 0)
                        {
                            res.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                        sb.Append(c);
                }
            }
            if (sb.Length > 0)
                res.Add(sb.ToString());
            return res.ToArray();
        }
        #endregion
    }
}
