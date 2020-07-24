async function saveOptions(e) {
    const host = document.querySelector('#host').value;
    const defaultRepositoryPath = document.querySelector('#defaultRepositoryPath').value;

    if (!host || !host.match(/^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/)) {
        alert('Due to browser limitations the host url must match /^(https?|wss?|file|ftp|\*):\/\/(\*|\*\.[^*/]+|[^*/]+)\/.*$/')
        return;
    }

    const permission = await browser.permissions.request({
        origins: [host]
    });

    if (!permission) {
        showMsg('Permission rejected. Configuration incomplete.', true);
        restoreOptions();
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

function pingHost() {
    browser.runtime.sendMessage({ method: 'ping' })
        .then(e => showMsg(`${e.receiver}-pong!`))
        .catch(e => showMsg(e['message'] || JSON.stringify(e)))
}

function showMsg(msg, error = false) {
    let msgArea = document.querySelector('#msgArea');
    msgArea.textContent = msg;
    if (error)
        msgArea.classList.add('error');
    setTimeout(() => { 
        msgArea.textContent = '';
        if (msgArea.classList.contains('error'))
            msgArea.classList.remove('error');
     }, 2000);
}

function init() {
    document.addEventListener('DOMContentLoaded', restoreOptions);
    document.querySelector('#saveOptions').addEventListener('click', saveOptions);
    document.querySelector('#pingHost').addEventListener('click', pingHost);
}

init();