
# Jira Integration Tool

This project has the goal of reducing friction and saving time when working on Jira issues.

Currently it is focused on a local repository integration, allowing the automation of picking out the right branch, fetching, checking out, applying stashed changes and running custom scripts after checkout - Everything with just a single click, directly from the web browser - your repository no longer feels isolated from Jira! It is also possible to fully customize the checkout logic through bash scripting.

This tool consists of a browser addon and a native executable. The addon injects elements into jira pages and communicates through [native messaging](https://developer.chrome.com/apps/nativeMessaging) with the native executable performing actions on your local repository.

#### Currently only a single button is being injected which triggers the checkout on interaction:
![](/screenshot.png)

# Installation

The python host supports Windows, macOS and mainstream Linux systems.


There is also a .NET Framework version of the host for Windows without the python dependency.

The browser addon was tested on Firefox and Chrome, but you won't be able to install it on chrome since Google forbids
installations outside the web store.

## Installation steps

1. Download the latest release from the [github release page](https://github.com/gpa/JiraIntegrationTool/releases). 
2. Unzip the package and install the .xpi addon with firefox.
3. Execute the python script (or the windows executable) without arguments. Make sure not to move the files after that or do it again. 
4. You should be all set up by now! If you encounter any problems, make sure to restart your browser. 
5. You can modify the checkout logic in the ``actions.sh`` file.
