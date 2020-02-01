using Newtonsoft.Json;

namespace JiraIntegrationTool.NetFrameworkHost.Requests
{
    class CheckoutBranchRequest : BaseRequest
    {
        [JsonProperty("defaultRepositoryPath")]
        public string DefaultRepositoryPath { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("issueId")]
        public string IssueId { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("issueUrl")]
        public string IssueUrl { get; set; }

        [JsonProperty("issueTitle")]
        public string IssueTitle { get; set; }

        [JsonProperty("issueType")]
        public string IssueType { get; set; }

        [JsonProperty("issuePriority")]
        public string IssuePriority { get; set; }
    }
}
