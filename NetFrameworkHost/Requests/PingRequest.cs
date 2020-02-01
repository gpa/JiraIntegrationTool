using Newtonsoft.Json;

namespace JiraIntegrationTool.NetFrameworkHost.Requests
{
    class PingRequest : BaseRequest
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }
    }
}
