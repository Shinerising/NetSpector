using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NetSpector
{
    /// <summary>
    /// INI配置文件读写相关
    /// </summary>
    public class INI
    {

        /// <summary>
        /// 写入INI配置文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件位置</param>
        /// <returns>写入结果</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        /// <summary>
        /// 读取INI配置文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">默认值</param>
        /// <param name="retVal">字符串构建对象</param>
        /// <param name="size">读取最大长度</param>
        /// <param name="filePath">文件位置</param>
        /// <returns>读取结果</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 默认INI配置文件路径
        /// </summary>
        public static string IniFilePath = AppDomain.CurrentDomain.BaseDirectory + Process.GetCurrentProcess().ProcessName + @".ini";

        /// <summary>
        /// 读取ini文件
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <returns>读取结果</returns>
        public static T ReadIni<T>(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, string.Empty, temp, 255, IniFilePath);
            if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
            {
                string result = temp.ToString();
                return (T)Convert.ChangeType(result, typeof(T));
            }
            else
            {
                int.TryParse(temp.ToString(), out int result);
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// 读取ini文件
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">未读取到值时的默认值</param>
        /// <returns>读取结果</returns>
        public static T ReadIni<T>(string section, string key, string def)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, def, temp, 255, IniFilePath);
            if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
            {
                string result = temp.Length == 0 ? def : temp.ToString();
                return (T)Convert.ChangeType(result, typeof(T));
            }
            else
            {
                int.TryParse(temp.Length == 0 ? def : temp.ToString(), out int result);
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// 读取ini文件
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">未读取到值时的默认值</param>
        /// <param name="path">文件位置</param>
        /// <returns>读取结果</returns>
        public static T ReadIni<T>(string section, string key, string def, string path)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, def, temp, 255, path);
            if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
            {
                string result = temp.ToString();
                return (T)Convert.ChangeType(result, typeof(T));
            }
            else
            {
                int.TryParse(temp.ToString(), out int result);
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void WriteIni<T>(string section, string key, T value)
        {
            if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
            {
                WritePrivateProfileString(section, key, value.ToString(), IniFilePath);
            }
            else if (Type.GetTypeCode(typeof(T)) == TypeCode.Boolean)
            {
                WritePrivateProfileString(section, key, (bool)(object)value ? "1" : "0", IniFilePath);
            }
            else
            {
                WritePrivateProfileString(section, key, value.ToString(), IniFilePath);
            }
        }
        /// <summary>
        /// 删除节
        /// </summary>
        /// <param name="section">节</param>
        /// <returns>删除结果</returns>
        public static bool DeleteSection(string section)
        {
            return WritePrivateProfileString(section, null, null, IniFilePath);
        }

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <returns>删除结果</returns>
        public static bool DeleteKey(string section, string key)
        {
            return WritePrivateProfileString(section, key, null, IniFilePath);
        }
    }
}