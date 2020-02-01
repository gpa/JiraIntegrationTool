using JiraIntegrationTool.NetFrameworkHost.Requests;
using JiraIntegrationTool.NetFrameworkHost.Responses;
using System;
using System.Diagnostics;
using System.IO;

namespace JiraIntegrationTool.NetFrameworkHost.Services
{
    class NativeHostService
    {
        private const string VersionString = "netframework-host-0.1.0-alpha";

        public PingResponse Ping(PingRequest pingRequest)
        {
            return new PingResponse
            {
                Receiver = VersionString
            };
        }

        public CheckoutBranchResponse CheckoutBranch(CheckoutBranchRequest checkoutBranchRequest)
        {
            var r = checkoutBranchRequest;
            RunAction($"checkoutBranch \"{r.DefaultRepositoryPath}\" {r.BranchId} {r.IssueId} \"{r.ProjectName}\" \"{r.IssueUrl}\" \"{r.IssueTitle}\" \"{r.IssueType}\" \"{r.IssuePriority}\"");
            return new CheckoutBranchResponse();
        }

        private void RunAction(string args)
        {
            Process process = new Process();
            process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramW6432%\\Git\\git-bash.exe");
            if (!File.Exists(process.StartInfo.FileName))
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%\\Git\\bin\\sh.exe");

            process.StartInfo.Arguments = $"./actions.sh {args}";
            process.Start();
        }
    }
}
