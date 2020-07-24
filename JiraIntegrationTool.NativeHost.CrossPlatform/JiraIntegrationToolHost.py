#!/usr/bin/env python3

import os
import sys
import json
import struct
import platform
import shutil

versionString = 'python-host-1.0'

class NativeHostService:
    def ping(self, params):
        return { 'receiver': versionString }

    def checkoutBranch(self, params):
        params = (rf'checkoutBranch \"{params["defaultRepositoryPath"]}\" {params["branchId"]} {params["issueId"]}')
        self.runAction(params)

    def runAction(self, params):
        # We want to run the script in a terminal window since sometimes the commands require user interaction.
        # Also this way we don't have to report the progress back to the addon.
        # Unfortunately though there is no universal way of doing it.
        if platform.system() == 'Windows':
            bash = os.path.expandvars(r"%ProgramW6432%\Git\git-bash.exe")
            if not os.path.isfile(bash):
                bash = os.path.expandvars(r"%ProgramFiles(x86)%\Git\bin\sh.exe")
            os.system(rf'start "" "{bash}" -c "./actions.sh {params}"')
        elif platform.system() == 'Darwin':
            os.system(f'osascript -e \'tell app "Terminal" to do script "sh {os.getcwd()}/actions.sh {params}"\'') 
        else:
            knownTerminals = ['konsole', 'gnome-terminal', 'xfce4-terminal', 'mate-terminal', 'terminator', 'x-terminal-emulator']
            for executable in knownTerminals:
                if shutil.which(executable):
                    os.system(rf'{executable} -e "./actions.sh {params}"')
                    return
            raise RuntimeError('Failed to find a known terminal in your PATH.')
            
class EnvironmentInstaller:
    def installNativeMessagingManifest(self):
        from os import path
        from pathlib import Path

        platformName = platform.system()
        localScriptPath = path.realpath(__file__)
        manifestName = 'jiraintegrationtool.nativehost'
        localManifestPath = path.join(path.dirname(localScriptPath), f'{manifestName}.json')
        
        remoteManifestPath = localManifestPath
        if platformName == 'Linux':
            remoteManifestPath = path.expanduser(f'~/.mozilla/native-messaging-hosts/{manifestName}.json')
        elif platformName == 'Darwin':
            remoteManifestPath = path.expanduser(f'~/Library/Application Support/Mozilla/NativeMessagingHosts/{manifestName}.json')

        if platformName == 'Windows':
            regKey = rf"HKEY_CURRENT_USER\SOFTWARE\Mozilla\NativeMessagingHosts\{manifestName}"
            os.system(rf'reg add "{regKey}" /ve /t REG_SZ /d "{localManifestPath}" /f')
            pythonExecutionProxyPath = path.join(path.dirname(localScriptPath), rf'{manifestName}.bat')
            with open(pythonExecutionProxyPath, 'w+') as batFile:
                batFile.write(f'@echo off\r\ncall py -3 {localScriptPath} %0 %1 %2')
            localScriptPath = pythonExecutionProxyPath

        with open(localManifestPath, 'r+') as localManifestFile:
            manifestData = json.load(localManifestFile)
            manifestData['path'] = localScriptPath
            localManifestFile.seek(0)
            json.dump(manifestData, localManifestFile, indent=4)
            localManifestFile.truncate()
            if localManifestPath != remoteManifestPath:
                Path(path.dirname(remoteManifestPath)).mkdir(parents=True, exist_ok=True)
                with open(remoteManifestPath, 'w+') as remoteManifestFile:
                    json.dump(manifestData, remoteManifestFile, indent = 4)
                    remoteManifestFile.truncate()

        os.chmod('actions.sh', 0o744)
        print('Manifest installed succesfully.')

class NativeMessagingChannel:

    def __init__(self, nativeHostService):
        self.nativeHostService = nativeHostService

    def getMessage(self):
        rawLength = sys.stdin.buffer.read(4)
        if len(rawLength) == 0:
            sys.exit(0)
        messageLength = struct.unpack('@I', rawLength)[0]
        message = sys.stdin.buffer.read(messageLength).decode('utf-8')
        return json.loads(message)

    def sendMessage(self, messageContent):
        encodedContent = json.dumps(messageContent).encode('utf-8')
        encodedLength = struct.pack('@I', len(encodedContent))
        sys.stdout.buffer.write(encodedLength)
        sys.stdout.buffer.write(encodedContent)
        sys.stdout.buffer.flush()

    def sendResponse(self, incomingMessage, outgoingMessage, isError = False):
        if not isError:
            self.sendMessage({ 'jsonrpc': '2.0', 'result': outgoingMessage, 'id': incomingMessage['id']})
        else:
            self.sendMessage({ 'jsonrpc': '2.0', 'error': outgoingMessage, 'id': incomingMessage['id']})

    def waitForMessages(self):
        while True:
            incomingMessage = self.getMessage()
            try:
                serviceMethod = getattr(self.nativeHostService, incomingMessage['method'])
                serviceMethodResult = serviceMethod(incomingMessage['params'])
                self.sendResponse(incomingMessage, serviceMethodResult)
            except BaseException as error:
                self.sendResponse(incomingMessage, 'An exception occurred: {}'.format(error), isError = True)
 
if len(sys.argv) > 1:
    NativeMessagingChannel(NativeHostService()).waitForMessages()
else:
    EnvironmentInstaller().installNativeMessagingManifest()