#!/bin/bash

# This file is for your custom logic!
# Note that this script is also executed on Windows through the git for windows bash emulator
# but you can call a powershell script from here if you really need to.

# The arguments passed to this script are:
# $1 - name of the currently performed action, you most likely want to ignore that.
# $2 - Default repository path as configured in the extension options page
# $3 - Name of the branch to checkout
# $4 - Id of the jira issue
# $5 - Name of the jira project
# $6 - Full URL to the issue
# $7, $8, $9 - issue title, issue type and issue priority respectively

checkoutBranch() 
{
    cd $2
    
    if [ "`git branch --list $3`" ]
    then
        git checkout $3
    else
        git fetch origin $3
        git checkout --track origin/$3
    fi
}

onError()
{
    echo "Action failed" >&2
    /bin/bash --login -i
}

trap 'onError' 0
set -e
checkoutBranch $*
trap : 0