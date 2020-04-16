
# Jira Integration Tool

This project has the goal of reducing friction and saving time when working on Jira issues.

Currently it is focused on a local repository integration, allowing the automation of picking out the right branch, fetching, checking out, applying stashed changes and running custom scripts after checkout - Everything with just a single click, directly from the web browser - your repository no longer feels isolated from Jira! It is also possible to fully customize the checkout logic through bash scripting.

This tool consists of a browser addon and a native executable. The addon injects elements into jira pages and communicates through [native messaging](https://developer.chrome.com/apps/nativeMessaging) with the native executable performing actions on your local repository.

#### Currently only a single button is being injected which triggers the checkout on interaction:
![](/screenshot.png)
Fuhrter integrations, such as displaying git stashes for the issue, are planned in the future.
# Installation

The python host supports Windows, macOS and mainstream Linux systems.


There is also a .NET Framework version of the host for Windows without the python dependency.

The browser addon works with Firefox.

Chrome support is planned but not a priority.

## Installation steps

1. Download the latest release from the [github release page](https://github.com/gpa/JiraIntegrationTool/releases). 
2. Unzip the package and execute the windows executable (or the python script) without any arguments. Make sure not to move the files after that or do it again. 
3. Install the .xpi addon by opening it with firefox.
4. Click twice the "ping host" link in the options page. You should see a succesful pong message after the second time. If not, try restarting your browser.
5. You're all set up! The checkout logic can be modified in the ``actions.sh`` file.
