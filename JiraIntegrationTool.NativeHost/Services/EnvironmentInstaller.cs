using System.IO;
using JiraIntegrationTool.NativeHost.Utils;
using Newtonsoft.Json.Linq;

namespace JiraIntegrationTool.NativeHost.Services
{
    public static class EnvironmentInstaller
    {
        private const string _manifestName = "jiraintegrationtool.nativehost";
        private const string _executableName = "JiraIntegrationTool.NativeHost.exe";

        public static void Install()
        {
            var manifestPath = PrepareManifest();
            SetRegistryKeys(manifestPath);
        }

        public static void Uninstall()
        {
            RemoveRegistryKeys();
        }

        private static string PrepareManifest()
        {
            var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), $"{_manifestName}.json");
            var executablePath = Path.Combine(Directory.GetCurrentDirectory(), _executableName);

            var manifestText = File.ReadAllText(manifestPath);
            var manifestJson = JObject.Parse(manifestText);
            manifestJson["path"] = executablePath;
            File.WriteAllText(manifestPath, manifestJson.ToString());

            return manifestPath;
        }

        private static void SetRegistryKeys(string manifestPath)
        {
            if (IsFirefoxInstalled())
                RegistryHelper.SetDefaultKeyValue(GetFirefoxRegistryKey(), GetFirefoxBugWorkaroundManifestPath(manifestPath));

            if (IsChromeInstalled())
                RegistryHelper.SetDefaultKeyValue(GetChromeRegistryKey(), manifestPath);
        }

        private static void RemoveRegistryKeys()
        {
            RegistryHelper.RemoveKey(GetFirefoxRegistryKey());
            RegistryHelper.RemoveKey(GetChromeRegistryKey());
        }

        private static bool IsFirefoxInstalled()
        {
            return RegistryHelper.KeyExists("SOFTWARE\\Mozilla\\Firefox");
        }

        private static bool IsChromeInstalled()
        {
            return RegistryHelper.KeyExists("SOFTWARE\\Google\\Chrome");
        }

        private static string GetFirefoxRegistryKey()
        {
            return $"SOFTWARE\\Mozilla\\NativeMessagingHosts\\{_manifestName}";
        }

        private static string GetChromeRegistryKey()
        {
            return $"SOFTWARE\\Google\\Chrome\\NativeMessagingHosts\\{_manifestName}";
        }

        private static string GetFirefoxBugWorkaroundManifestPath(string originalManifestPath)
        {
            // As soon as we add the 'allowed_origins' json property to make the extension work with chrome,
            // the firefox connection stops working... Create a copy of the manifest just for firefox without
            // the property.
            var ffManifestPath = Path.Combine(Path.GetDirectoryName(originalManifestPath), $"{_manifestName}_ff.json");
            if (File.Exists(ffManifestPath))
                File.Delete(ffManifestPath);

            File.Copy(originalManifestPath, ffManifestPath);
            var manifestText = File.ReadAllText(ffManifestPath);
            var manifestJson = JObject.Parse(manifestText);
            manifestJson.Remove("allowed_origins");
            File.WriteAllText(ffManifestPath, manifestJson.ToString());
            return ffManifestPath;
        }
    }
}
