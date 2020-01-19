const connection = chrome.runtime.connectNative('JiraIntegrationToolHost');

function onMessageFromContentScript(message) {
  sendMessageToNativeHost(message);
}

function onMessageFromNativeHost(message) {
  chrome.notifications.create({
    'type': 'basic',
    'title': 'Jira integration tool',
    'message': JSON.stringify(message)
  });
}

function sendMessageToNativeHost(message) {
  try {
    connection.postMessage(message);
  } catch (e) {
    chrome.notifications.create({
      'type': 'basic',
      'title': 'Failed to communicate with a native host',
      'message': e.message
    });
  }
}

connection.onMessage.addListener(onMessageFromNativeHost);
chrome.runtime.onMessage.addListener(onMessageFromContentScript);