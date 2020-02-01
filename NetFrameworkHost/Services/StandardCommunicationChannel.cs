using JiraIntegrationTool.NetFrameworkHost.Requests;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace JiraIntegrationTool.NetFrameworkHost.Services
{
    class StandardCommunicationChannel
    {
        private readonly NativeHostService _nativeHostService;

        public StandardCommunicationChannel(NativeHostService nativeHostService)
        {
            _nativeHostService = nativeHostService;
        }

        public void WaitForMessages()
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
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            var messageJson = JObject.Parse(message);

            if (messageJson.Value<string>("method") == "checkoutBranch")
            {
                var checkoutBranchRequest = messageJson["params"].ToObject<CheckoutBranchRequest>();
                _nativeHostService.CheckoutBranch(checkoutBranchRequest);
            }
            else if(messageJson.Value<string>("method") == "ping")
            {
                var pingRequest = messageJson["params"].ToObject<PingRequest>();
                _nativeHostService.Ping(pingRequest);
            }
            else
            {
                throw new ArgumentException(message);
            }
        }
    }
}
