using Newtonsoft.Json;

namespace JiraIntegrationTool.NetFrameworkHost.Responses
{
    class PingResponse : BaseResponse
    {
        [JsonProperty("receiver")]
        public string Receiver { get; set; }
    }
}
