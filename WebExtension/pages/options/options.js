async function saveOptions(e) {
    const host = document.querySelector('#host').value;
    const repositoryPath = document.querySelector('#repositoryPath').value;

    if (!host || !host.match(/^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/)) {
        alert('Due to browser limitations the host url must match /^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/')
        return;
    }

    browser.storage.local.set({
        host,
        repositoryPath
    });

    restoreOptions();
    browser.runtime.sendMessage({ method: 'updateConfiguration' });
    e.preventDefault();
}

async function restoreOptions() {
    const options = await browser.storage.local.get(['host', 'repositoryPath']);
    document.querySelector('#host').value = options.host || 'https://jira.example.com/browse/*';
    document.querySelector('#repositoryPath').value = options.repositoryPath || `C:\\Development\\YourRepository`;
    if (!options['host']) {
        document.querySelector('.input-required').classList.remove('hidden');
    } else {
        document.querySelector('.input-required').classList.add('hidden');
    }
}

document.addEventListener('DOMContentLoaded', restoreOptions);
document.querySelector('form').addEventListener('submit', saveOptions);