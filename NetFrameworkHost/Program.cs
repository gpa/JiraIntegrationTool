using JiraIntegrationTool.NetFrameworkHost.Services;
using System;

namespace JiraIntegrationTool.NetFrameworkHost
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                try
                {
                    InstallationService.InstallNativeMessagingManifest();
                    Console.WriteLine("Manifest installed succesfully.");
                } 
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.ReadLine();
            }
            else
            {
                var nativeHostService = new NativeHostService();
                var nativeMessagingChannel = new StandardCommunicationChannel(nativeHostService);
                nativeMessagingChannel.WaitForMessages();
            }
        }
    }
}
