namespace JiraIntegrationTool.NativeHost.Responses
{
    public class PingResponse : BaseResponse
    {
        public string ReceiverName { get; set; }

        public string ReceiverVersion { get; set; }
    }
}
