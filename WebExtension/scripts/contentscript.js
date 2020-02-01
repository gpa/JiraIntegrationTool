const script = document.createElement('script');
script.src = browser.runtime.getURL('scripts/pagescript.js');
script.onload = () => this.remove();
(document.head || document.documentElement).appendChild(script);

document.addEventListener('branchCheckoutTriggeredEvent', e => {
  browser.runtime.sendMessage({ method: 'checkoutBranch', params: e.detail });
});