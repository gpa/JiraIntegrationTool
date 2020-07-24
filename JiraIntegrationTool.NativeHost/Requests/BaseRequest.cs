using System;
using System.Runtime.Serialization;

namespace JiraIntegrationTool.NativeHost.Requests
{
    [Serializable]
    public abstract class BaseRequest
    {
        public BaseRequest(SerializationInfo info, StreamingContext context)
        {
        }
    }
}
