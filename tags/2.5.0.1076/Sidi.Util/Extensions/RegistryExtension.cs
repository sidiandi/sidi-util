using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;
using System.Reflection;

namespace Sidi.Extensions
{
    public static class RegistryExtension
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Dictionary<string, RegistryKey> roots;

        static readonly string sep = @"\";
        
        /// <summary>
        /// Opens a registry key using the full key path, as functions Registry.GetValue, Registry.SetValue
        /// </summary>
        /// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
        /// <param name="writable">Determines if the key can be used for write operations.</param>
        /// <returns></returns>
        public static RegistryKey OpenKey(string keyName, bool writable = false)
        {
            var p = keyName.Split(new[]{sep}, StringSplitOptions.None);
            
            if (roots == null)
            {
                roots = new[] { Registry.ClassesRoot, Registry.CurrentConfig, Registry.CurrentUser, Registry.LocalMachine, Registry.PerformanceData, Registry.Users }
                    .ToDictionary(_ => _.Name, _ => _);
            }

            RegistryKey rootKey = null;
            if (roots.TryGetValue(p[0], out rootKey))
            {
                return rootKey.OpenSubKey(p.Skip(1).Join(sep), writable);
            }
            else
            {
                throw new ArgumentOutOfRangeException("keyName", keyName, "{0} is not a valid registry root".F(p[0]));
            }
        }

        public static void DeleteValue(string keyName, string valueName)
        {
            using (var k = OpenKey(keyName, true))
            {
                k.DeleteValue(valueName);
            }
        }

        public static void EnsureValueNotExists(string keyName, string valueName)
        {
            using (var k = OpenKey(keyName, true))
            {
                if (k != null)
                {
                    if (k.GetValue(valueName) != null)
                    {
                        k.DeleteValue(valueName);
                    }
                }
            }
        }

        public static string Cat(params string[] parts)
        {
            return parts.Join(sep);
        }

        public static string GetRegistryKeyUserSoftware(this Assembly assembly)
        {
            var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            return RegistryExtension.Cat(
                Registry.CurrentUser.ToString(),
                "Software",
                company,
                product
                );
        }
    }
}
