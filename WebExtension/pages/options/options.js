async function saveOptions(e) {
    const host = document.querySelector('#host').value;
    const defaultRepositoryPath = document.querySelector('#defaultRepositoryPath').value;

    if (!host || !host.match(/^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/)) {
        alert('Due to browser limitations the host url must match /^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/')
        return;
    }

    await browser.storage.local.set({
        host,
        defaultRepositoryPath
    });

    e.preventDefault();
    location.reload();
}

async function restoreOptions() {
    const options = await browser.storage.local.get(['host', 'defaultRepositoryPath']);
    document.querySelector('#host').value = options.host || 'https://jira.example.com/browse/*';
    document.querySelector('#defaultRepositoryPath').value = options.defaultRepositoryPath || `C:\\Development\\YourRepository`;
    if (!options['host']) {
        document.querySelector('.input-required').classList.remove('hidden');
    } else {
        document.querySelector('.input-required').classList.add('hidden');
    }
}

document.addEventListener('DOMContentLoaded', restoreOptions);
document.querySelector('form').addEventListener('submit', saveOptions);
document.querySelector('#pingHost').addEventListener('click', () => {
    browser.runtime.sendMessage({ method: 'ping' }).then(e => {
        document.querySelector('#pingResult').textContent = `${e.receiver}-pong!`;
        setTimeout(() => document.querySelector('#pingResult').textContent = '', 3000);
    }).catch(e => {
        document.querySelector('#pingResult').textContent = e['message'] || JSON.stringify(e);
        setTimeout(() => document.querySelector('#pingResult').textContent = '', 3000);
    })
});