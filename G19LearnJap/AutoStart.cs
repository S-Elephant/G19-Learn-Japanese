using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace G19LearnJap
{
    public class AutoStart
    {
        private const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string KeyName = "G19LearnJap";
        static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;  // Or the EXE path.

        /// <summary>
        /// Sets the autostart value for the assembly.
        /// </summary>
        public static void SetAutoStart()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
            key.SetValue(KeyName, AssemblyLocation);
        }

        /// <summary>
        /// Returns whether auto start is enabled.
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION);
            if (key == null)
                return false;

            string value = (string)key.GetValue(KeyName);
            if (value == null)
                return false;

            return (value == AssemblyLocation);
        }

        /// <summary>
        /// Unsets the autostart value for the assembly.
        /// </summary>
        public static void UnSetAutoStart()
        {
            if (IsAutoStartEnabled())
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
                key.DeleteValue(KeyName);
            }
        }
    }
}
