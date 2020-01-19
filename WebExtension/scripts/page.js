function onBranchCheckoutButtonClicked(event) {
  document.dispatchEvent(new CustomEvent('branchCheckoutTriggeredEvent', {
    detail: {
      branchId: $(event.currentTarget).data('branch-id')
    }
  }));
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
  mutations.forEach(mutation => {
    mutation.addedNodes.forEach(addedNode => {
      if (addedNode.id == 'devstatus-branch-detail-dialog' && addedNode.classList.contains('jira-dialog-content-ready')) {
        dialogMutationObserver = new MutationObserver(onBranchDialogMutation);
        dialogMutationObserver.observe(addedNode, { childList: true, subtree: true });
      }
    });
  });
}

new MutationObserver(onBodyMutation)
  .observe(document.body, {
    childList: true,
    subtree: false
  });