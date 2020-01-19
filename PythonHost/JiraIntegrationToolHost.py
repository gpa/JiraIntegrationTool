import os
import sys
import json
import struct
import platform
import subprocess

versionString = 'python-host-0.1.0'

class NativeHostService:
    def ping(self, params):
        return { 'receiver': versionString }

    def checkoutBranch(self, params):
        branchId = params['branchId']
        os.chdir('absolute/path/to/your/repository')
        os.system(f'git fetch origin/{branchId}')
        os.system(f'git checkout --track origin/{branchId}')
        return { 'status': f'checked out {branchId}' }

class EnvironmentInstaller:
    def installNativeMessagingManifest(self):
        from os import path
        from platform import system
        from pathlib import Path

        platformName = system()
        localScriptPath = path.realpath(__file__)
        localManifestPath = path.join(path.dirname(localScriptPath), 'JiraIntegrationToolHost.json')
        remoteManifestPath = localManifestPath

        if platformName == 'Linux':
            remoteManifestPath = path.join(path.expanduser('~'), '.mozilla/native-messaging-hosts/JiraIntegrationToolHost.json')
        elif platformName == 'Darwin':
            remoteManifestPath = path.join(path.expanduser('~'), 'Library/Application Support/Mozilla/NativeMessagingHosts/JiraIntegrationToolHost.json')
        elif platformName == 'Windows':
            os.system(r'reg add "HKEY_CURRENT_USER\SOFTWARE\Mozilla\NativeMessagingHosts\JiraIntegrationToolHost" /ve /t REG_SZ /d "'+localManifestPath+'" /f')
            pythonExecutionProxyPath = path.dirname(localScriptPath) + r'\JiraIntegrationToolHost.bat'
            with open(pythonExecutionProxyPath, 'w+') as batFile:
                batFile.write("@echo off\r\ncall python3 " + localScriptPath + r" %0 %1 %2")
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

    def sendResponse(self, incomingMessage, outgoingMessage):
        self.sendMessage({ 'jsonrpc': '2.0', 'result': outgoingMessage, 'id': incomingMessage['id']})

    def sendErrorResponse(self, incomingMessage, outgoingError):
        self.sendMessage({ 'jsonrpc': '2.0', 'error': outgoingError, 'id': incomingMessage['id']})

    def handleMessage(self, incomingMessage):
        return getattr(self.nativeHostService, incomingMessage['method'])(incomingMessage['params'])

    def waitForMessages(self):
        while True:
            incomingMessage = self.getMessage()
            try:
                serviceMethod = getattr(self.nativeHostService, incomingMessage['method'])
                serviceMethodResult = serviceMethod(incomingMessage['params'])
                self.sendResponse(incomingMessage, serviceMethodResult)
            except BaseException as error:
                self.sendErrorResponse(incomingMessage, 'An exception occurred: {}'.format(error))
 
if len(sys.argv) > 1:
    NativeMessagingChannel(NativeHostService()).waitForMessages()
else:
    EnvironmentInstaller().installNativeMessagingManifest()