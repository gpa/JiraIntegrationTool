function onBranchCheckoutButtonClicked(event) {
  const branchDetails = {
      branchId: $(event.currentTarget).data('branch-id'),
      issueId: $("#key-val").text(),
  }
  document.dispatchEvent(new CustomEvent('branchCheckoutTriggeredEvent', { detail: branchDetails }));
}

function createBranchCheckoutButton(branchId) {
  return $(`<a data-branch-id="${branchId}" style="padding-left: 10px"><span 
    class="aui-lozenge aui-lozenge-overflow aui-lozenge-subtle aui-lozenge-complete">Checkout</span></a>`)
    .click(onBranchCheckoutButtonClicked)
}

function onBranchDialogMutation(mutations) {
  mutations.forEach(mutation => {
    mutation.addedNodes.forEach(addedNode => {
      $(addedNode).find('.branch-row .branch .branch-link').each((i, branchLinkNode, arr) => {
        createBranchCheckoutButton(branchLinkNode.innerText).insertAfter(branchLinkNode);
      });
    })
  })
}

function onBodyMutation(mutations) {
  for (let mutation of mutations) {
    for (let addedNode of mutation.addedNodes) {
      if (addedNode.id == 'devstatus-branch-detail-dialog' && addedNode.classList.contains('jira-dialog-content-ready')) {
        dialogMutationObserver = new MutationObserver(onBranchDialogMutation);
        dialogMutationObserver.observe(addedNode, { childList: true, subtree: true });
        return;
      }
    }
  }
}

$(document).ready(function() {
  new MutationObserver(onBodyMutation)
  .observe(document.body, {
    childList: true,
    subtree: false
  });
});
