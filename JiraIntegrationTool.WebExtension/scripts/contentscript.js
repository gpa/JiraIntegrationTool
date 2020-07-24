const script = document.createElement('script');
script.src = browser.runtime.getURL('scripts/pagescript.js');
(document.head || document.documentElement).appendChild(script);
document.addEventListener('branchCheckoutTriggeredEvent', e => {
  browser.runtime.sendMessage({ method: 'checkoutBranch', params: e.detail });
});