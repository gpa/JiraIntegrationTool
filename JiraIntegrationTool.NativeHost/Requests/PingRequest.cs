using System;
using System.Runtime.Serialization;

namespace JiraIntegrationTool.NativeHost.Requests
{
    [Serializable]
    public class PingRequest : BaseRequest
    {
        public string SenderName { get; set; }

        public string SenderVersion { get; set; }

        public PingRequest(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
