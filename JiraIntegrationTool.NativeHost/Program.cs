using System;
using System.Linq;
using JiraIntegrationTool.NativeHost.Services;

namespace JiraIntegrationTool.NativeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    EnvironmentInstaller.Install();
                    Console.WriteLine("Manifest installed succesfully.");
                    Console.ReadLine();
                }
                else if (args.Contains("--uninstall"))
                {
                    EnvironmentInstaller.Uninstall();
                    Console.WriteLine("Manifest successfully uninstalled.");
                    Console.ReadLine();
                }
                else
                {
                    using (var inputStream = Console.OpenStandardInput())
                    {
                        using (var outputStream = Console.OpenStandardOutput())
                        {
                            var nativeService = new NativeHostService();
                            var communicationChannel = new NativeMessagingJsonRpcService<NativeHostService>(inputStream, outputStream, nativeService);
                            communicationChannel.Open();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.ReadLine();
            }
        }
    }
}
