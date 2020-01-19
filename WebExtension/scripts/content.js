const script = document.createElement('script');
script.src = chrome.runtime.getURL('scripts/page.js');
script.onload = () => this.remove();
(document.head || document.documentElement).appendChild(script);

let rpcId = 0;
document.addEventListener('branchCheckoutTriggeredEvent', e => {
  chrome.runtime.sendMessage({
    jsonrpc: '2.0',
    method: 'checkoutBranch',
    params: { branchId: e.detail.branchId },
    id: rpcId++
  });
});