
# Jira Integration Tool

With the Jira Integration Tool switching issues no longer feels like a pain but like an frictionless and enjoyable experience.
Automate the actions of picking out the right branch, fetching, checking out, applying stashed changes and running custom scripts after checkout with just a single click, directly from your web browser.

This tool consists of a browser addon and a native executable. The addon injects elements into jira pages and communicates through [native messaging](https://developer.chrome.com/apps/nativeMessaging) with the native executable performing actions on your local repository.

#### Currently only a single button is being injected which triggers the checkout on interaction:
![](/screenshot.png)
Fuhrter integrations, such as displaying git stashes for the issue, are planned in the future.
# Installation

JIT supports Firefox and Chrome on Windows and depends only on the .NET Framework 4.5+

There is also a cross platform python version of the host, although only supporting Firefox.

## Installation steps

1. Download the latest release from the [github release page](https://github.com/gpa/JiraIntegrationTool/releases). 
2. Unzip the package and execute the windows executable (or the python script) without any arguments. Make sure not to move the files after that or do it again. 
3. Install the .xpi addon by opening it with firefox.
4. Click the "ping host" link in the options page to make sure your connection to the host works. If it doesn't, try pinging it again. Restart your browser or re-execute the host .exe as a last resort.
5. You should be all set up! The checkout logic can be customized in the ``actions.sh`` file.
