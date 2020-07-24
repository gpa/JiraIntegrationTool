using Microsoft.Win32;

namespace JiraIntegrationTool.NativeHost.Utils
{
    public static class RegistryHelper
    {
        public static bool KeyExists(string key)
        {
            var registryEntry = Registry.CurrentUser.OpenSubKey(key, false);
            if (registryEntry != null)
            {
                registryEntry.Close();
                return true;
            }

            return false;
        }

        public static void RemoveKey(string key)
        {
            Registry.CurrentUser.DeleteSubKeyTree(key, false);
        }

        public static void SetDefaultKeyValue(string key, string value)
        {
            var registryEntry = Registry.CurrentUser.OpenSubKey(key, true);
            if (registryEntry == null)
                registryEntry = Registry.CurrentUser.CreateSubKey(key);

            registryEntry.SetValue("", value);
            registryEntry.Close();
        }
    }
}
