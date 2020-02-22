using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace JiraIntegrationTool.NetFrameworkHost
{
    class Program
    {
        private const string VersionString = "netframework-host-1.0";

        private static void InstallNativeMessagingManifest()
        {
            void SetRegistryKey(string key, string value, string valueKey = "")
            {
                var registryEntry = Registry.CurrentUser.OpenSubKey(key, true);
                if (registryEntry == null)
                    registryEntry = Registry.CurrentUser.CreateSubKey(key);
                
                registryEntry.SetValue(valueKey, value);
                registryEntry.Close();
            }

            var manifestName = "io.github.gpa.jiraintegrationtool.host";
            var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), $"{manifestName}.json");
            var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "JiraIntegrationToolHost.exe");

            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(manifestJson);
            manifest["path"] = executablePath;
            File.WriteAllText(manifestPath, manifest.ToString());
            SetRegistryKey($"SOFTWARE\\Mozilla\\NativeMessagingHosts\\{manifestName}", manifestPath);
        }

        private static byte[] ReadBytes(Stream stream, int count)
        {
            var data = new byte[count];
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                    throw new EndOfStreamException($"End of stream reached with {remaining} bytes left to read");
                remaining -= read;
                offset += read;
            }

            return data;
        }

        private static void WaitForMessages()
        {
            using (Stream inputStream = Console.OpenStandardInput())
            {
                while (true)
                {
                    var messageLengthBytes = ReadBytes(inputStream, 4);
                    var messageLength = BitConverter.ToInt32(messageLengthBytes, 0);
                    byte[] messageBytes = ReadBytes(inputStream, messageLength);
                    var message = new string(Encoding.UTF8.GetChars(messageBytes, 0, messageBytes.Length));
                    var messageJson = JObject.Parse(message);
                    
                    try
                    {
                        var response = ProcessMessage(messageJson.Value<string>("method"), messageJson.GetValue("params"));
                        SendResponse(messageJson, response);
                    } 
                    catch(Exception e)
                    {
                        SendResponse(messageJson, e);
                    }
                }
            }
        }

        private static void SendResponse(JObject incomingMessage, dynamic result)
        {
            using (Stream outputStream = Console.OpenStandardOutput())
            {
                dynamic resultObject;
                if (result is Exception)
                    resultObject = new { jsonrpc = "2.0", error = result, id = incomingMessage.Value<string>("id") };
                else
                    resultObject = new { jsonrpc = "2.0", result, id = incomingMessage.Value<string>("id") };

                var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(resultObject);
                var resultBytes = Encoding.UTF8.GetBytes(resultJson);
                outputStream.Write(BitConverter.GetBytes(resultJson.Length), 0, 4);
                outputStream.Write(resultBytes, 0, resultBytes.Length);
                outputStream.Flush();
            }
        }

        private static dynamic ProcessMessage(string method, JToken @params)
        {
            if (method == "ping")
                return new { receiver = VersionString };

            if (method == "checkoutBranch")
            {
                var keys = new[] { "defaultRepositoryPath", "branchId", "issueId", "projectName", "issueUrl", "issueTitle", "issueType", "issuePriority" };
                RunAction($"checkoutBranch {string.Join(" ", keys.Select(x => $"\"{@params.Value<string>(x)}\""))}");
                return null;
            }
            
            throw new ArgumentException("Malformed or unrecognized input.");
        }

        private static void RunAction(string args)
        {
            // We want to run the script in a terminal window since sometimes the commands require user interaction.
            // Also this way we don't have to report the progress back to the addon.
            Process process = new Process();
            process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramW6432%\\Git\\git-bash.exe");
            if (!File.Exists(process.StartInfo.FileName))
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%\\Git\\bin\\sh.exe");

            process.StartInfo.Arguments = $"./actions.sh {args}";
            process.Start();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                try
                {
                    InstallNativeMessagingManifest();
                    Console.WriteLine("Manifest installed succesfully.");
                } 
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.ReadLine();
            }
            else
                WaitForMessages();
        }
    }
}
