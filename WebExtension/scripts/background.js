const versionString = 'web-extension-0.1.0-alpha';

class NativeHost {

  constructor() {
    this._connection = browser.runtime.connectNative('JiraIntegrationToolHost');
    this._connection.onMessage.addListener(this._onIncomingMessage.bind(this));
    this._rpcId = 0;
    this._pendingCalls = {}
  }

  ping(versionString) {
    return this._execute('ping', { sender: versionString });
  }

  checkoutBranch(branchWithIssueDetails) {
    return this._execute('checkoutBranch', branchWithIssueDetails);
  }

  _execute(method, params) {
    return new Promise((resolve, reject) => {
      try {
        this._connection.postMessage({
          jsonrpc: '2.0',
          method: method,
          params: params,
          id: this._rpcId++
        });
        this._pendingCalls[this._rpcId-1] = { resolve, reject };
      } catch(e) {
        reject(e);
      }
    })
  }

  _onIncomingMessage(message) {
    if (typeof message['id'] === 'undefined' || typeof this._pendingCalls[message.id] === 'undefined') {
      throw new Error(`Unexpected incoming message ${JSON.stringify(message)}`)
    }

    const binding = this._pendingCalls[message.id];
    delete this._pendingCalls[message.id];
    if (message['error'])
      binding.reject(message.error);
    else
      binding.resolve(message.result);
    }
}

class JiraIntegrationTool {

  constructor() {
    this.nativeHost = new NativeHost();
    this.updateConfiguration();
    browser.storage.onChanged.addListener(this.updateConfiguration.bind(this));
  }

  async checkoutBranch(branchWithIssueDetails) {
    branchWithIssueDetails['defaultRepositoryPath'] = this.options['defaultRepositoryPath'];
    await this._execute(async () => await this.nativeHost.checkoutBranch(branchWithIssueDetails));
  }

  async ping() {
    const response = await this._execute(async () => await this.nativeHost.ping(versionString));
    await this._showMessage(`${response.receiver} pong!`);
  }

  async updateConfiguration() {
    this.options = await browser.storage.local.get(['host', 'defaultRepositoryPath']);
    if (!this.options['host'] || !this.options['defaultRepositoryPath']) {
      await browser.runtime.openOptionsPage()
      return;
    }
    if (this.contentScript) {
      await this.contentScript.unregister();
    }
    this.contentScript = await browser.contentScripts.register({
      matches: [this.options['host']],
      js: [{
        file: 'scripts/contentscript.js'
      }],
      runAt: "document_end"
    });
  }

  async _execute(func) {
    try {
      const result = await func();
      return result;
    } catch(e) {
      await this._showMessage(e['message'] || JSON.stringify(e));
      return null;
    }
  }

  async _showMessage(message) {
    return browser.notifications.create({
      'type': 'basic',
      'title': 'Jira Integration Tool',
      'message': message
    });
  }
}

const jiraIntegrationToolService = new JiraIntegrationTool();
browser.runtime.onMessage.addListener(message => jiraIntegrationToolService[message['method']](message.params));