using System;
using System.Runtime.Serialization;

namespace JiraIntegrationTool.NativeHost.Requests
{
    [Serializable]
    public class CheckoutBranchRequest : BaseRequest
    {
        public string DefaultRepositoryPath { get; set; }

        public string BranchId { get; set; }

        public string IssueId { get; set; }

        public CheckoutBranchRequest(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
