using Microsoft.Win32;
using NUnit.Framework;
using Sidi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Extensions
{
    [TestFixture]
    public class RegistryExtensionTest : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void OpenKey()
        {
            using (var k = RegistryExtension.OpenKey(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows"))
            {
            }

            using (var k = RegistryExtension.OpenKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows"))
            {
            }
        }

        [Test, ExpectedException(ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void InvalidRoot()
        {
            using (var k = RegistryExtension.OpenKey(@"HKEY_LOCAL_MACHIN\SOFTWARE\Microsoft\Windows"))
            {
            }
        }

        [Test]
        public void DeleteValue()
        {
            var keyName = @"HKEY_CURRENT_USER\SOFTWARE\sidi-util\test";
            var valueName = @"testValue";
            var value = "hello";
            Registry.SetValue(keyName, valueName, value);
            Assert.AreEqual(value, Registry.GetValue(keyName, valueName, String.Empty));
            using (var k = RegistryExtension.OpenKey(keyName))
            {
                log.Info(k.ValueCount);
            }
            RegistryExtension.DeleteValue(keyName, valueName);
            Assert.AreEqual("defaultValue", Registry.GetValue(keyName, valueName, "defaultValue"));
        }
    }
}
