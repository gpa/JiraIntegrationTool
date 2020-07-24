using System;
using System.IO;
using System.Text;
using JiraIntegrationTool.NativeHost.Requests;
using JiraIntegrationTool.NativeHost.Responses;
using JiraIntegrationTool.NativeHost.Utils;
using Newtonsoft.Json.Linq;

namespace JiraIntegrationTool.NativeHost.Services
{
    class NativeMessagingJsonRpcService<TService>
    {
        private readonly TService _service;
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        public NativeMessagingJsonRpcService(Stream inputStream, Stream outputStream, TService service)
        {
            _service = service;
            _inputStream = inputStream;
            _outputStream = outputStream;
        }

        public void Open()
        {
            while (true)
                ReadMessage(_inputStream);
        }

        private void ReadMessage(Stream inputStream)
        {
            var callId = -1;
            try
            {
                var messagePreamble = StreamHelper.ReadBytes(inputStream, 4);
                var lengthOfIncomingMessage = BitConverter.ToInt32(messagePreamble, 0);
                byte[] receivedBytes = StreamHelper.ReadBytes(inputStream, lengthOfIncomingMessage);
                var receivedMessage = new string(Encoding.UTF8.GetChars(receivedBytes, 0, receivedBytes.Length));
                var parsedMessage = JObject.Parse(receivedMessage);
                var messageMethodName = parsedMessage.Value<string>("method");
                var normalizedMethodName = char.ToUpperInvariant(messageMethodName[0]) + messageMethodName.Substring(1);
                var messageMethodParams = parsedMessage["params"];

                var methodInfo = typeof(TService).GetMethod(normalizedMethodName);
                var parameters = methodInfo.GetParameters();

                if (parameters.Length != 1)
                    throw new ArgumentException($"Invalid signature of {methodInfo.Name}.");

                if (!(methodInfo.ReturnType.BaseType == typeof(BaseResponse)
                    && parameters[0].ParameterType.BaseType == typeof(BaseRequest)))
                    throw new ArgumentException($"Invalid signature of {methodInfo.Name}.");

                callId = parsedMessage.Value<int>("id");
                var request = messageMethodParams.ToObject(parameters[0].ParameterType);
                var response = methodInfo.Invoke(_service, new[] { request });
                SendResponse(callId, (BaseResponse)response);
            }
            catch (EndOfStreamException)
            {
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                SendResponse(callId, exception);
            }
        }

        private void SendResponse(int requestId, BaseResponse response)
        {
            dynamic resultJson = new { jsonrpc = "2.0", result = response, id = requestId };
            SendResponse(resultJson);
        }

        private void SendResponse(int requestId, Exception exception)
        {
            dynamic resultJson = new { jsonrpc = "2.0", error = new { message = exception.Message }, id = requestId };
            SendResponse(resultJson);
        }

        private void SendResponse(dynamic responseMessage)
        {
            var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(responseMessage);
            var resultBytes = Encoding.UTF8.GetBytes(resultJson);
            _outputStream.Write(BitConverter.GetBytes(resultJson.Length), 0, 4);
            _outputStream.Write(resultBytes, 0, resultBytes.Length);
            _outputStream.Flush();
        }
    }
}
