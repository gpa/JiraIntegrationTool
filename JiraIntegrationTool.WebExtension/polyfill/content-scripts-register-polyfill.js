/* https://github.com/fregante/content-scripts-register-polyfill @ v1.0.0 */
/*MIT License
Copyright (c) Federico Brigante <opensource@bfred.it> (bfred.it)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */
(function () {
    'use strict';

    function urlGlobToRegex(matchPattern) {
        return '^' + matchPattern
            .replace(/[.]/g, '\\.')
            .replace(/[?]/, '.')
            .replace(/^[*]:/, 'https?')
            .replace(/^(https[?]?:[/][/])[*]/, '$1[^/:]+')
            .replace(/[/][*]/, '/?.+')
            .replace(/[*]/g, '.+')
            .replace(/[/]/g, '\\/');
    }
    async function p(fn, ...args) {
        return new Promise((resolve, reject) => {
            fn(...args, result => {
                if (chrome.runtime.lastError) {
                    reject(chrome.runtime.lastError);
                }
                else {
                    resolve(result);
                }
            });
        });
    }
    async function isOriginPermitted(url) {
        return p(chrome.permissions.contains, {
            origins: [new URL(url).origin + '/*']
        });
    }
    async function wasPreviouslyLoaded(tabId, loadCheck) {
        const result = await p(chrome.tabs.executeScript, tabId, {
            code: loadCheck,
            runAt: 'document_start'
        });
        return result && result[0];
    }
    if (!chrome.contentScripts) {
        chrome.contentScripts = {
            async register(contentScriptOptions, callback) {
                const { js = [], css = [], allFrames, matchAboutBlank, matches, runAt } = contentScriptOptions;
                const loadCheck = `document[${JSON.stringify(JSON.stringify({ js, css }))}]`;
                const matchesRegex = new RegExp(matches.map(urlGlobToRegex).join('$') + '$');
                const listener = async (tabId, { status }) => {
                    if (status !== 'loading') {
                        return;
                    }
                    const { url } = await p(chrome.tabs.get, tabId);
                    if (!url ||
                        !matchesRegex.test(url) ||
                        !await isOriginPermitted(url) ||
                        await wasPreviouslyLoaded(tabId, loadCheck)
                    ) {
                        return;
                    }
                    for (const file of css) {
                        chrome.tabs.insertCSS(tabId, {
                            ...file,
                            matchAboutBlank,
                            allFrames,
                            runAt: runAt || 'document_start'
                        });
                    }
                    for (const file of js) {
                        chrome.tabs.executeScript(tabId, {
                            ...file,
                            matchAboutBlank,
                            allFrames,
                            runAt
                        });
                    }
                    chrome.tabs.executeScript(tabId, {
                        code: `${loadCheck} = true`,
                        runAt: 'document_start',
                        allFrames
                    });
                };
                chrome.tabs.onUpdated.addListener(listener);
                const registeredContentScript = {
                    async unregister() {
                        return p(chrome.tabs.onUpdated.removeListener.bind(chrome.tabs.onUpdated), listener);
                    }
                };
                if (typeof callback === 'function') {
                    callback(registeredContentScript);
                }
                return Promise.resolve(registeredContentScript);
            }
        };
    }

}());