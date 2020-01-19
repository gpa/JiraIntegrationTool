const script = document.createElement('script');
script.src = browser.runtime.getURL('scripts/page.js');
script.onload = () => this.remove();
(document.head || document.documentElement).appendChild(script);

document.addEventListener('branchCheckoutTriggeredEvent', e => {
  browser.runtime.sendMessage({ method: 'checkoutBranch', params: { branchId: e.detail.branchId } });
});