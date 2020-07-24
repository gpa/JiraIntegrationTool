using System.Reflection;
using JiraIntegrationTool.NativeHost.Requests;
using JiraIntegrationTool.NativeHost.Responses;
using JiraIntegrationTool.NativeHost.Utils;

namespace JiraIntegrationTool.NativeHost.Services
{
    public class NativeHostService
    {
        public PingResponse Ping(PingRequest _)
        {
            return new PingResponse()
            {
                ReceiverName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title,
                ReceiverVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
        }

        public CheckoutBranchResponse CheckoutBranch(CheckoutBranchRequest checkoutBranchRequest)
        {
            var r = checkoutBranchRequest;
            var args = $"checkoutBranch {r.DefaultRepositoryPath} {r.BranchId} {r.IssueId}";
            ShellHelper.RunCommandInShellWindow($"./actions.sh {args}");
            return new CheckoutBranchResponse();
        }
    }
}
