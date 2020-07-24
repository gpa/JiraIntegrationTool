using System;
using System.Diagnostics;
using System.IO;

namespace JiraIntegrationTool.NativeHost.Utils
{
    public static class ShellHelper
    {
        public static void RunCommandInShellWindow(string args)
        {
            // we want to run a script in a terminal window since sometimes the commands require user interaction.
            // Also this way we don't have to report the progress back to the addon.
            Process process = new Process();
            process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramW6432%\\Git\\git-bash.exe");
            if (!File.Exists(process.StartInfo.FileName))
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%\\Git\\bin\\sh.exe");

            process.StartInfo.Arguments = args;
            process.Start();
        }
    }
}
