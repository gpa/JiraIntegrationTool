using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace JiraIntegrationTool.NetFrameworkHost
{
    class Program
    {
        private static void EnsureEnvironmentIsSetup()
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

            var path = "@TODO";
            var mozillaRegistry = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mozilla");
            if (mozillaRegistry != null)
            {
                mozillaRegistry.Close();
                SetRegistryKeyIfDoesNotExist("SOFTWARE\\Mozilla\\NativeMessagingHosts\\JiraIntegrationToolHost", path);
            }
        }

        private static void WaitForMessages(string[] args)
        {
            Stream inputStream = Console.OpenStandardInput();
            byte[] tempBuffer = new byte[4];
            while (true)
            {
                int prefixRead = inputStream.Read(tempBuffer, 0, 4);
                if (prefixRead == 0)
                {
                    Thread.Sleep(500);
                    continue;
                }

                if (prefixRead != 4)
                    Environment.Exit(0);

                int messageLength = BitConverter.ToInt32(tempBuffer, 0);
                byte[] buffer = new byte[messageLength];
                messageLength = inputStream.Read(buffer, 0, messageLength);
                var message = new string(Encoding.UTF8.GetChars(buffer, 0, messageLength));
            }
        }

        static void Main(string[] args)
        {
            EnsureEnvironmentIsSetup();
            WaitForMessages(args);
        }
    }
}
