const versionString = 'web-extension-1.1';

class JiraIntegrationTool {
  
  _nativeHostServiceId = 'jiraintegrationtool.nativehost';
  _optionDefaultRepositoryPath = 'defaultRepositoryPath';
  _optionHost = 'host';

  constructor() {
    this._nativeHostConnection = null;
    this._pendingRpcs = {};
    this._lastRpcId = 0;
    this._options = {};
    this._actionRateTimeLock = 0;
    this._onConfigurationChanged();
    this._establishNativeHostCommunication();
    browser.storage.onChanged.addListener(this._onConfigurationChanged.bind(this));
  }

  ping() {
    return this._sendNativeHostMessage('ping', { sender: `web-extension-${browser.runtime.getManifest().version}` });
  }

  checkoutBranch(branchDetails) {
    if (!this._setActionExecutionLock()) return;
    branchDetails['defaultRepositoryPath'] = this._options[this._optionDefaultRepositoryPath];
    return this._sendNativeHostMessage('checkoutBranch', branchDetails);
  }

  async _onConfigurationChanged() {
    console.log('config changed');
    this._options = await browser.storage.local.get([this._optionHost, this._optionDefaultRepositoryPath]);
    if (!(await this._enforceValidConfiguration()))
      return;
    this._registerContentScript();
  }

  async _enforceValidConfiguration() {
    if (!this._options[this._optionHost] || !this._options[this._optionDefaultRepositoryPath]) {
      await browser.runtime.openOptionsPage()
      return false;
    }
    return true;
  }

  async _registerContentScript() {
    console.log('registering');
    if (this._contentScript) {
      await this._contentScript.unregister();
    }
    this._contentScript = await browser.contentScripts.register({
      matches: [
        this._options[this._optionHost]
      ],
      js: [
        {
          file: 'polyfill/browser-polyfill.js'
        },
        {
          file: 'scripts/contentscript.js'
        }],
      runAt: "document_end"
    });
  }

  _setActionExecutionLock() {
    if (this._actionRateTimeLock && this._actionRateTimeLock > Date.now())
      return false;
    this._actionRateTimeLock = Date.now() + 3000;
    return true;
  }

  _establishNativeHostCommunication() {
    this._nativeHostConnection = browser.runtime.connectNative(this._nativeHostServiceId);
    this._nativeHostConnection.onMessage.addListener(this._onNativeHostMessage.bind(this));
  }

  _sendNativeHostMessage(method, params) {
    return new Promise((resolve, reject) => {
      try {
        this._nativeHostConnection.postMessage({
          jsonrpc: '2.0',
          method: method,
          params: params,
          id: this._lastRpcId++
        });
        this._pendingRpcs[this._lastRpcId - 1] = { resolve, reject };
      } catch (e) {
        this._onFailedNativeHostMessage(e);
        reject(e);
      }
    })
  }

  _onNativeHostMessage(message) {
    if (typeof message['id'] === 'undefined' || typeof this._pendingRpcs[message.id] === 'undefined') {
      this._logError(`Unexpected incoming message ${JSON.stringify(message)}`);
      return;
    }
    const rpcBinding = this._pendingRpcs[message.id];
    delete this._pendingRpcs[message.id];
    if (message['error']) {
      this._logError(message.error);
      rpcBinding.reject(message.error);
    }
    else
      rpcBinding.resolve(message.result);
  }

  _onFailedNativeHostMessage(error) {
    this._logError(error);
    this._establishNativeHostCommunication();
  }

  _logError(error) {
    console.log(error);
    return browser.notifications.create({
      'type': 'basic',
      'title': 'Jira Integration Tool Error',
      'message': typeof error === 'string' ? error : error['message'] || JSON.stringify(error)
    });
  }
}

const jiraIntegrationTool = new JiraIntegrationTool();
browser.runtime.onMessage.addListener(message => jiraIntegrationTool[message['method']](message.params));