using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.IO;

namespace JiraIntegrationTool.NetFrameworkHost.Services
{
    public static class InstallationService
    {
        public static void InstallNativeMessagingManifest()
        {
            void SetRegistryKeyIfDoesNotExist(string key, string value, string valueKey = "")
            {
                var registryEntry = Registry.CurrentUser.OpenSubKey(key, true);
                if (registryEntry == null)
                {
                    var registryKey = Registry.CurrentUser.CreateSubKey(key);
                    registryKey.SetValue(valueKey, value);
                    registryKey.Close();
                }
            }

            var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "JiraIntegrationToolHost.json");
            var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "JiraIntegrationToolHost.exe");

            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(manifestJson);
            manifest["path"] = executablePath;
            File.WriteAllText(manifestPath, manifest.ToString());
            SetRegistryKeyIfDoesNotExist("SOFTWARE\\Mozilla\\NativeMessagingHosts\\JiraIntegrationToolHost", manifestPath);
        }
    }
}
