let scriptPromise;

function loadScript() {
    if (window.google?.accounts?.id) {
        return Promise.resolve();
    }

    if (!scriptPromise) {
        scriptPromise = new Promise((resolve, reject) => {
            const existing = document.querySelector('script[data-google-gsi]');
            if (existing) {
                existing.addEventListener('load', () => resolve());
                existing.addEventListener('error', () => reject(new Error('GIS load failed')));
                return;
            }

            const script = document.createElement('script');
            script.src = 'https://accounts.google.com/gsi/client';
            script.async = true;
            script.defer = true;
            script.dataset.googleGsi = 'true';
            script.onload = () => resolve();
            script.onerror = () => reject(new Error('GIS load failed'));
            document.head.appendChild(script);
        });
    }

    return scriptPromise;
}

export async function isOnline() {
    return navigator.onLine;
}

export async function getClientEnvironment() {
    let serviceWorkerActive = false;
    if ('serviceWorker' in navigator) {
        const registration = await navigator.serviceWorker.getRegistration();
        serviceWorkerActive = registration?.active != null;
    }

    return {
        userAgent: navigator.userAgent,
        language: navigator.language,
        viewportWidth: window.innerWidth,
        viewportHeight: window.innerHeight,
        online: navigator.onLine,
        serviceWorkerSupported: 'serviceWorker' in navigator,
        serviceWorkerActive,
    };
}
