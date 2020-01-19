const versionString = 'web-extension-0.1.0';

class NativeHost {

  constructor() {
    this._connection = browser.runtime.connectNative('JiraIntegrationToolHost');
    this._connection.onMessage.addListener(this._onIncomingMessage.bind(this));
    this._rpcId = 0;
    this._pendingCalls = {}
  }

  ping() {
    return this._execute('ping', { sender: versionString });
  }

  checkoutBranch(branchId) {
    return this._execute('checkoutBranch', { branchId: branchId });
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

const nativeHost = new NativeHost();
browser.runtime.onMessage.addListener(async (message) => {
  function showMessage(message) {
    browser.notifications.create({
      'type': 'basic',
      'title': 'Jira integration tool',
      'message': message
    });
  }
  
  try {
    const result = await nativeHost[message.method](message.params);
    showMessage(result);
  } catch(e) {
    showMessage(JSON.stringify(e));
  }
});